namespace AgileObjects.AgileMapper.Configuration.Inline
{
    internal delegate TTarget InlineMappingExecutor<in TSource, TTarget>(TSource source, TTarget target);
}