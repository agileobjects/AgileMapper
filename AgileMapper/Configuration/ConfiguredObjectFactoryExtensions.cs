namespace AgileObjects.AgileMapper.Configuration
{
    internal static class ConfiguredObjectFactoryExtensions
    {
        public static bool IsCreationFactory(this ConfiguredObjectFactory factory)
            => factory.ConfigInfo.Get<FactoryType>() == FactoryType.Creation;
    }
}