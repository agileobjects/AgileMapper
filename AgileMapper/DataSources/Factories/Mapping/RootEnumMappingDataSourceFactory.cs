namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class RootEnumMappingDataSourceFactory : MappingDataSourceFactoryBase
    {
        public RootEnumMappingDataSourceFactory()
            : base(new EnumMappingExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.IsRoot && mappingData.MapperData.TargetType.GetNonNullableType().IsEnum();
    }
}
