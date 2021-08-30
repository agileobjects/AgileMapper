namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal static class DefaultRootMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IEntryPointMappingContext context)
            => new RootObjectMapperKey(context);
    }
}