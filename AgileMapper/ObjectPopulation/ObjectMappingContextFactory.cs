namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        private delegate IObjectMappingContext ObjectMappingContextCreator<in TSource, in TTarget, in TInstance>(
            TSource source,
            IQualifiedMember sourceMember,
            TTarget target,
            IQualifiedMember targetMember,
            TInstance existing,
            int? enumerableIndex,
            MappingContext mappingContext);

        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)));
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)));

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