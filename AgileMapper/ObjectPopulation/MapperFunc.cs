namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TTarget MapperFunc<TSource, TTarget>(
        ObjectMappingContext<TSource, TTarget> objectMappingContext);
}