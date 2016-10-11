namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal static class ObjectMappingContextDataFactory
    {
        public static IObjectMappingContextData ForRoot<TSource, TTarget>(
            TSource source,
            TTarget target,
            IMappingContext mappingContext)
        {
            var sourceMember = mappingContext.MapperContext.RootMemberFactory.RootSource<TSource>();
            var targetMember = mappingContext.MapperContext.RootMemberFactory.RootTarget<TTarget>();

            return Create(
                source,
                target,
                null,
                sourceMember,
                targetMember,
                mappingContext);
        }

        public static IObjectMappingContextData ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingContextData parent)
        {
            var sourceMember = parent.MapperData.GetSourceMemberFor(targetMemberName, dataSourceIndex);
            var targetMember = parent.MapperData.GetTargetMemberFor(targetMemberName);

            return Create(
                source,
                target,
                enumerableIndex,
                sourceMember,
                targetMember,
                parent.MappingContext,
                parent);
        }

        public static IObjectMappingContextData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IObjectMappingContextData parent)
        {
            return Create(
                source,
                target,
                enumerableIndex,
                parent.MapperData.SourceElementMember,
                parent.MapperData.TargetElementMember,
                parent.MappingContext,
                parent);
        }

        private static IObjectMappingContextData Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            IMappingContext mappingContext,
            IObjectMappingContextData parent = null)
        {
            sourceMember = Verify(sourceMember, source);
            targetMember = Verify(targetMember, sourceMember, source, target, enumerableIndex, mappingContext, parent);
            var runtimeTypesAreTheSame = (sourceMember.Type == typeof(TDeclaredSource)) && (targetMember.Type == typeof(TDeclaredTarget));

            if (runtimeTypesAreTheSame)
            {
                return new ObjectMappingContextData<TDeclaredSource, TDeclaredTarget>(
                    source,
                    target,
                    enumerableIndex,
                    sourceMember,
                    targetMember,
                    true,
                    mappingContext,
                    parent);
            }

            var constructionFunc = GetContextDataCreator<TDeclaredSource, TDeclaredTarget>(sourceMember, targetMember);

            return constructionFunc.Invoke(
                source,
                target,
                enumerableIndex,
                sourceMember,
                targetMember,
                false,
                mappingContext,
                parent);
        }

        private delegate IObjectMappingContextData ContextDataCreator<in TSource, in TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            bool runtimeTypesAreTheSame,
            IMappingContext mappingContext,
            IObjectMappingContextData parent);

        private static ContextDataCreator<TDeclaredSource, TDeclaredTarget> GetContextDataCreator<TDeclaredSource, TDeclaredTarget>(
            IQualifiedMember sourceMember,
            IQualifiedMember targetMember)
        {
            var constructorKey = DeclaredAndRuntimeTypesKey.ForMappingDataConstructor(
                typeof(TDeclaredSource),
                typeof(TDeclaredTarget),
                sourceMember.Type,
                targetMember.Type);

            var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(constructorKey, k =>
            {
                var dataType = typeof(ObjectMappingContextData<,>)
                    .MakeGenericType(k.RuntimeSourceType, k.RuntimeTargetType);

                var sourceParameter = Parameters.Create(k.DeclaredSourceType, "source");
                var targetParameter = Parameters.Create(k.DeclaredTargetType, "target");

                var constructorCall = Expression.New(
                    dataType.GetPublicInstanceConstructors().First(),
                    sourceParameter.GetConversionTo(k.RuntimeSourceType),
                    targetParameter.GetConversionTo(k.RuntimeTargetType),
                    Parameters.EnumerableIndexNullable,
                    Parameters.SourceMember,
                    Parameters.TargetMember,
                    Parameters.RuntimeTypesAreTheSame,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingContextData);

                var constructionLambda = Expression.Lambda<ContextDataCreator<TDeclaredSource, TDeclaredTarget>>(
                    constructorCall,
                    sourceParameter,
                    targetParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.SourceMember,
                    Parameters.TargetMember,
                    Parameters.RuntimeTypesAreTheSame,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingContextData);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }

        private static IQualifiedMember Verify<TSource>(IQualifiedMember sourceMember, TSource source)
            => CheckSourceRuntimeType(sourceMember) ? sourceMember.WithType(source.GetRuntimeSourceType()) : sourceMember;

        private static bool CheckSourceRuntimeType(IQualifiedMember sourceMember)
        {
            if (sourceMember.IsEnumerable)
            {
                return !sourceMember.Type.IsGenericType();
            }

            return !sourceMember.Type.IsSealed();
        }

        private static QualifiedMember Verify<TSource, TTarget>(
            QualifiedMember targetMember,
            IQualifiedMember sourceMember,
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IMappingContext mappingContext,
            IBasicMappingContextData parent)
        {
            if (!CheckTargetRuntimeType(targetMember))
            {
                return targetMember;
            }

            var targetMemberType = mappingContext.MapperContext.UserConfigurations.DerivedTypePairs
                .GetDerivedTypeOrNull(
                    source,
                    target,
                    enumerableIndex,
                    sourceMember,
                    targetMember,
                    mappingContext,
                    parent) ?? target.GetRuntimeTargetType(sourceMember.Type);

            return targetMember.WithType(targetMemberType);
        }

        private static bool CheckTargetRuntimeType(IQualifiedMember targetMember)
        {
            if (targetMember.IsEnumerable)
            {
                return targetMember.Type.IsInterface();
            }

            return !targetMember.Type.IsSealed();
        }
    }
}