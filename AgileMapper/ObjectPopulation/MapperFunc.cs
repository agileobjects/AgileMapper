namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TTarget MapperFunc<TSource, TTarget>(ObjectMappingContextData<TSource, TTarget> data);
}