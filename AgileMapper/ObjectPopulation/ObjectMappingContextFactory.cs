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
            QualifiedMember targetMember,
            TInstance existing,
            int? enumerableIndex,
            MappingContext mappingContext);

        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)), mappingContext.MapperContext.NamingSettings);
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)), mappingContext.MapperContext.NamingSettings);

            return Create(ObjectMappingCommand.Create(
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
            ObjectMappingCommand<TDeclaredSource, TDeclaredTarget, TDeclaredInstance> command)
        {
            var funcKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1}),{2}({3}),{4}({5}): ObjectMappingContextConstructor",
                typeof(TDeclaredSource).FullName,
                command.SourceMember.Type.FullName,
                typeof(TDeclaredTarget).FullName,
                command.TargetMember.Type.FullName,
                typeof(TDeclaredInstance).FullName,
                command.ExistingTargetInstanceMember.Type.FullName);

            var constructionFunc = command.MappingContext.GlobalContext.Cache.GetOrAdd(funcKey, k =>
            {
                var sourceParameter = Parameters.Create<TDeclaredSource>("source");
                var targetParameter = Parameters.Create<TDeclaredTarget>("target");
                var existingInstanceParameter = Parameters.Create<TDeclaredInstance>("existingInstance");

                var contextType = typeof(ObjectMappingContext<,,>).MakeGenericType(
                    command.SourceMember.Type,
                    command.TargetMember.Type,
                    command.ExistingTargetInstanceMember.Type);

                var constructorCall = Expression.New(
                    contextType.GetConstructors().First(),
                    sourceParameter.GetConversionTo(command.SourceMember.Type),
                    Parameters.SourceMember,
                    targetParameter.GetConversionTo(command.TargetMember.Type),
                    Parameters.TargetMember,
                    existingInstanceParameter.GetConversionTo(command.ExistingTargetInstanceMember.Type),
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
                command.Source,
                command.SourceMember,
                command.Target,
                command.ExistingTargetInstanceMember,
                command.ExistingTargetInstance,
                command.EnumerableIndex,
                command.MappingContext);
        }
    }
}