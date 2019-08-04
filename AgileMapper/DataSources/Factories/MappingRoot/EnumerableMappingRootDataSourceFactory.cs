namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class EnumerableMappingRootDataSourceFactory : MappingRootDataSourceFactoryBase
    {
        public EnumerableMappingRootDataSourceFactory()
            : base(new EnumerableMappingExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsEnumerable;
    }
}