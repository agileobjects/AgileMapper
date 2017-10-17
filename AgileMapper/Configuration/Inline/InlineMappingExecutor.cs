namespace AgileObjects.AgileMapper.Configuration.Inline
{
    internal delegate TTarget InlineMappingExecutor<TSource, TTarget>(
        TSource source,
        TTarget target,
        MappingExecutor<TSource> initiatingExecutor);
}