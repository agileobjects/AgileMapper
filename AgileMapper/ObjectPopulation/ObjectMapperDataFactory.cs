namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class ObjectMapperDataFactory
    {
        private delegate ObjectMapperData OmdCreator(
            MapperContext mapperContext,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData parent);

        public static ObjectMapperData CreateRoot<TDeclaredSource, TDeclaredTarget>(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> rootInstanceData)
        {
            var namingSettings = rootInstanceData.MappingContext.MapperContext.NamingSettings;

            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)), namingSettings);
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)), namingSettings);

            return Create(ObjectMapperDataFactoryBridge.Create(
                rootInstanceData,
                sourceMember,
                targetMember));
        }

        public static ObjectMapperData Create(ObjectMapperDataFactoryBridge command)
        {
            return new ObjectMapperData(
                command.MappingContext.MapperContext,
                command.MappingContext.RuleSet,
                command.SourceMember,
                command.TargetMember,
                null);

            //var omcConstructorKey = DeclaredAndRuntimeTypesKey.From(command);

            //var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(omcConstructorKey, _ =>
            //{
            //    var contextType = typeof(ObjectMapperData<,>)
            //        .MakeGenericType(command.SourceMember.Type, command.TargetMember.Type);

            //    var constructorCall = Expression.New(
            //        contextType.GetConstructors().First(),
            //        Parameters.SourceMember,
            //        Parameters.TargetMember,
            //        Parameters.MappingContext);

            //    var constructionLambda = Expression.Lambda<OmdCreator>(
            //        constructorCall,
            //        Parameters.SourceMember,
            //        Parameters.TargetMember,
            //        Parameters.MappingContext);

            //    return constructionLambda.Compile();
            //});

            //return constructionFunc.Invoke(
            //    command.SourceMember,
            //    command.TargetMember,
            //    command.MappingContext);
        }
    }
}