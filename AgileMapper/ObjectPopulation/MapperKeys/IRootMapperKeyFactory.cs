namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    internal interface IRootMapperKeyFactory
    {
        ObjectMapperKeyBase CreateRootKeyFor<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData);
    }
}