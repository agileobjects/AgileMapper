namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(source, target);
            var sourceMember = QualifiedMember.From(Member.RootSource(runtimeTypes.Item1));
            var targetMember = QualifiedMember.From(Member.RootTarget(runtimeTypes.Item2));

            return Create(new ObjectMappingRequest<TDeclaredSource, TDeclaredTarget, TDeclaredTarget>(
                source,
                sourceMember,
                target,
                targetMember,
                target,
                targetMember,
                null,
                mappingContext));
        }

        public static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget, TDeclaredMember>(
            TDeclaredSource source,
            TDeclaredTarget target,
            IQualifiedMember childTargetMember,
            TDeclaredMember childMemberValue,
            MappingContext mappingContext)
        {
            Type sourceMemberRuntimeType;
            IQualifiedMember qualifiedSourceMember;

            if (mappingContext.CurrentObjectMappingContext.HasSource(source))
            {
                qualifiedSourceMember = mappingContext.CurrentObjectMappingContext.SourceMember;
                sourceMemberRuntimeType = qualifiedSourceMember.Type;
            }
            else
            {
                sourceMemberRuntimeType = source.GetRuntimeSourceType();

                var childMemberContext = new MemberMappingContext(childTargetMember, mappingContext.CurrentObjectMappingContext);

                qualifiedSourceMember = mappingContext
                    .MapperContext
                    .DataSources
                    .GetSourceMemberFor(childMemberContext);

                qualifiedSourceMember = qualifiedSourceMember.WithType(sourceMemberRuntimeType);
            }

            var runtimeTypes = Tuple.Create(
                sourceMemberRuntimeType,
                typeof(TDeclaredTarget),
                childTargetMember.Type);

            return Create(
                source,
                target,
                childMemberValue,
                runtimeTypes,
                qualifiedSourceMember,
                childTargetMember,
                mappingContext);
        }

        public static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget existingElement,
            int enumerableIndex,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(sourceElement, existingElement);

            return Create(
                sourceElement,
                existingElement,
                existingElement,
                runtimeTypes,
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.SourceMember, runtimeTypes.Item1),
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.TargetMember, runtimeTypes.Item2),
                mappingContext,
                enumerableIndex);
        }

        private static IQualifiedMember GetEnumerableElementMember(IQualifiedMember enumerableMember, Type runtimeType)
            => enumerableMember.Append(enumerableMember.Type.CreateElementMember().WithType(runtimeType));

        private static Tuple<Type, Type, Type> GetRuntimeTypes<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            var sourceType = source.GetRuntimeSourceType();
            var targetType = existing.GetRuntimeTargetType(sourceType);

            return Tuple.Create(sourceType, targetType, targetType);
        }

        private delegate IObjectMappingContext ObjectMappingContextCreator<in TSource, in TTarget, in TInstance>(
            TSource source,
            IQualifiedMember sourceMember,
            TTarget target,
            IQualifiedMember targetMember,
            TInstance existing,
            int? enumerableIndex,
            MappingContext mappingContext);

        private static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget, TInstance>(
            TDeclaredSource source,
            TDeclaredTarget target,
            TInstance existing,
            Tuple<Type, Type, Type> runtimeTypes,
            IQualifiedMember sourceMember,
            IQualifiedMember targetMember,
            MappingContext mappingContext,
            int? enumerableIndex = null)
        {
            return null/*Create(
                new ObjectMappingRequest<TDeclaredSource, TDeclaredTarget, TInstance>(
                    source,
                    sourceMember,
                    target,
                    mappingContext.CurrentObjectMappingContext?.TargetMember ?? typeof(TDeclaredTarget),
                    existing,
                    targetMember,
                    enumerableIndex,
                    mappingContext))*/;
        }

        public static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget, TDeclaredInstance>(
            ObjectMappingRequest<TDeclaredSource, TDeclaredTarget, TDeclaredInstance> request)
        {
            var funcKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1}),{2}({3}),{4}({5}): ObjectMappingContextConstructor",
                typeof(TDeclaredSource).FullName,
                request.SourceMember.Type.FullName,
                typeof(TDeclaredTarget).FullName,
                request.TargetMember.Type.FullName,
                typeof(TDeclaredInstance).FullName,
                request.ExistingTargetInstanceMember.Type.FullName);

            var constructionFunc = request.MappingContext.GlobalContext.Cache.GetOrAdd(funcKey, k =>
            {
                var sourceParameter = Parameters.Create<TDeclaredSource>("source");
                var targetParameter = Parameters.Create<TDeclaredTarget>("target");
                var existingInstanceParameter = Parameters.Create<TDeclaredInstance>("existingInstance");

                var contextType = typeof(ObjectMappingContext<,,>).MakeGenericType(
                    request.SourceMember.Type,
                    request.TargetMember.Type,
                    request.ExistingTargetInstanceMember.Type);

                var constructorCall = Expression.New(
                    contextType.GetConstructors().First(),
                    sourceParameter.GetConversionTo(request.SourceMember.Type),
                    Parameters.SourceMember,
                    targetParameter.GetConversionTo(request.TargetMember.Type),
                    Parameters.TargetMember,
                    existingInstanceParameter.GetConversionTo(request.ExistingTargetInstanceMember.Type),
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

                var constructionLambda = Expression
                    .Lambda<ObjectMappingContextCreator<TDeclaredSource, TDeclaredTarget, TDeclaredInstance>>(
                        constructorCall,
                        sourceParameter,
                        Parameters.SourceMember,
                        targetParameter,
                        Parameters.TargetMember,
                        existingInstanceParameter,
                        Parameters.EnumerableIndexNullable,
                        Parameters.MappingContext);

                return constructionLambda.Compile();
            });

            return constructionFunc.Invoke(
                request.Source,
                request.SourceMember,
                request.Target,
                request.ExistingTargetInstanceMember,
                request.ExistingTargetInstance,
                request.EnumerableIndex,
                request.MappingContext);
        }
    }
}