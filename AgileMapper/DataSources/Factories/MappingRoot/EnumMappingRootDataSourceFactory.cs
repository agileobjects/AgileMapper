namespace AgileObjects.AgileMapper.DataSources.Factories.MappingRoot
{
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class EnumMappingRootDataSourceFactory : MappingRootDataSourceFactoryBase, IMappingRootDataSourceFactory
    {
        public EnumMappingRootDataSourceFactory()
            : base(new EnumMappingExpressionFactory())
        {
        }

        public bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetType.GetNonNullableType().IsEnum();
    }
}
