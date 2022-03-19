namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using Extensions.Internal;
    using ObjectPopulation;
    using Queryables;

    internal class QueryProjectionDataSourceFactory : MappingDataSourceFactoryBase
    {
        public QueryProjectionDataSourceFactory()
            : base(new QueryProjectionExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData)
        {
            return mappingData.IsRoot &&
                  (mappingData.MappingContext.RuleSet.Name == Constants.Project) &&
                   mappingData.MapperData.TargetMember.IsEnumerable &&
                   mappingData.MapperData.SourceType.IsQueryable();
        }
    }
}