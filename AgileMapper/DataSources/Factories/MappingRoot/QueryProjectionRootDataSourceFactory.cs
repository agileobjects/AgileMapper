namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using Extensions.Internal;
    using ObjectPopulation;
    using Queryables;

    internal class QueryProjectionRootDataSourceFactory : MappingRootDataSourceFactoryBase, IMappingRootDataSourceFactory
    {
        public QueryProjectionRootDataSourceFactory()
            : base(QueryProjectionExpressionFactory.Instance)
        {
        }

        public bool IsFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            return mapperData.IsRoot &&
                   mapperData.TargetMember.IsEnumerable &&
                   (mappingData.MappingContext.RuleSet.Name == Constants.Project) &&
                   mapperData.SourceType.IsQueryable();
        }
    }
}
