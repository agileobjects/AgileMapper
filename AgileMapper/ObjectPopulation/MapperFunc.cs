namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TTarget MapperFunc<in TSource, TTarget>(IObjectMappingData<TSource, TTarget> mappingData);
}