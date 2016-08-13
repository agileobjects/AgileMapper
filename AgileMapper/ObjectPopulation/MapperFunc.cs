namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal delegate TTarget MapperFunc<TSource, TTarget>(ObjectMappingData<TSource, TTarget> data);
}