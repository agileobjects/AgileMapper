namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal struct RootMapperKeyFactory : IRootMapperKeyFactory
    {
        public ObjectMapperKeyBase CreateRootKeyFor<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            return new RootObjectMapperKey(mappingData.MappingTypes, mappingData.MappingContext)
            {
                MappingData = mappingData
            };
        }
    }
}