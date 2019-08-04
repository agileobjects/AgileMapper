namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using ObjectPopulation;
    using ObjectPopulation.ComplexTypes;

    internal class ComplexTypeMappingRootDataSourceFactory : MappingRootDataSourceFactoryBase
    {
        public ComplexTypeMappingRootDataSourceFactory()
            : base(new ComplexTypeMappingExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData) => true;
    }
}