namespace AgileObjects.AgileMapper.ObjectPopulation
{
    //internal delegate TTarget MapperFunc<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData);

    internal delegate TTarget MapperFunc<in TSource, TTarget>(
        TSource source,
        TTarget target,
        IMappingExecutionContext context);
}