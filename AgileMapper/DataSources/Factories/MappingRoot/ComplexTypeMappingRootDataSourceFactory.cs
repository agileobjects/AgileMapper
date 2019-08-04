namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;
    using ObjectPopulation.ComplexTypes;

    internal class ComplexTypeMappingRootDataSourceFactory : MappingRootDataSourceFactoryBase, IMappingRootDataSourceFactory
    {
        public ComplexTypeMappingRootDataSourceFactory()
            : base(new ComplexTypeMappingExpressionFactory())
        {
        }

        public bool IsFor(IObjectMappingData mappingData) => true;
    }
}