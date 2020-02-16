namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class EnumerableMappingDataSourceFactory : MappingDataSourceFactoryBase
    {
        public EnumerableMappingDataSourceFactory()
            : base(new EnumerableMappingExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsEnumerable;
    }
}