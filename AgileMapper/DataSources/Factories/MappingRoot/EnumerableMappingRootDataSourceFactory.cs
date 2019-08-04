namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;

    internal class EnumerableMappingRootDataSourceFactory : MappingRootDataSourceFactoryBase, IMappingRootDataSourceFactory
    {
        public EnumerableMappingRootDataSourceFactory()
            : base(new EnumerableMappingExpressionFactory())
        {
        }

        public bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsEnumerable;
    }
}