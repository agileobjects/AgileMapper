namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TInstance MapperFunc<TSource, TTarget, TInstance>(
        ObjectMappingContext<TSource, TTarget, TInstance> objectMappingContext);
}