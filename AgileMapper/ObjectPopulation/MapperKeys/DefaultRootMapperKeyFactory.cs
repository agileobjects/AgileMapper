namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal static class DefaultRootMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(MappingExecutionContextBase2 context)
            => new RootObjectMapperKey(context);
    }
}