namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal static class DefaultRootMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IMapperKeyData data)
            => new RootObjectMapperKey(data);
    }
}