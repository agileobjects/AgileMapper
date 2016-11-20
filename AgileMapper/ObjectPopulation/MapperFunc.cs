namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TTarget MapperFunc<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData);
}