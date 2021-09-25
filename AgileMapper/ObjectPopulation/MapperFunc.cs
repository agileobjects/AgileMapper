namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal delegate TTarget MapperFunc<in TSource, TTarget>(
        TSource source,
        TTarget target,
        IMappingExecutionContext context);
}