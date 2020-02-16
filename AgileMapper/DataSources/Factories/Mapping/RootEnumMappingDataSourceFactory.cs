namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

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
