namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        private delegate IObjectMappingContext ObjectMappingContextCreator<in TSource, in TTarget>(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext);

        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)), mappingContext.MapperContext.NamingSettings);
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)), mappingContext.MapperContext.NamingSettings);

            return Create(ObjectMappingContextFactoryBridge.Create(
                sourceMember,
                source,
                targetMember,
                target,
                default(int?),
                mappingContext));
        }

        public static IObjectMappingContext Create<TSource, TTarget>(
            ObjectMappingContextFactoryBridge<TSource, TTarget> command)
        {
            var funcKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1}),{2}({3}): ObjectMappingContextConstructor",
                typeof(TSource).FullName,
                command.SourceMember.Type.FullName,
                typeof(TTarget).FullName,
                command.TargetMember.Type.FullName);

            var constructionFunc = command.MappingContext.GlobalContext.Cache.GetOrAdd(funcKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var targetParameter = Parameters.Create<TTarget>("target");

                var contextType = typeof(ObjectMappingContext<,>)
                    .MakeGenericType(command.SourceMember.Type, command.TargetMember.Type);

                var constructorCall = Expression.New(
                    contextType.GetConstructors().First(),
                    Parameters.SourceMember,
                    sourceParameter.GetConversionTo(command.SourceMember.Type),
                    Parameters.TargetMember,
                    targetParameter.GetConversionTo(command.TargetMember.Type),
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

                var constructionLambda = Expression
                    .Lambda<ObjectMappingContextCreator<TSource, TTarget>>(
                        constructorCall,
                        Parameters.SourceMember,
                        sourceParameter,
                        Parameters.TargetMember,
                        targetParameter,
                        Parameters.EnumerableIndexNullable,
                        Parameters.MappingContext);

                return constructionLambda.Compile();
            });

            return constructionFunc.Invoke(
                command.SourceMember,
                command.Source,
                command.TargetMember,
                command.Target,
                command.EnumerableIndex,
                command.MappingContext);
        }
    }
}