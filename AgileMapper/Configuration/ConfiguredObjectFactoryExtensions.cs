namespace AgileObjects.AgileMapper.Configuration
{
    internal static class ConfiguredObjectFactoryExtensions
    {
        public static bool IsMappingFactory(this ConfiguredObjectFactory factory)
            => factory.ConfigInfo.Get<FactoryType>() == FactoryType.Mapping;
    }
}