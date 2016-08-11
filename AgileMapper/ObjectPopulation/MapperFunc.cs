namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal delegate TTarget MapperFunc<TSource, TTarget>(MappingData<TSource, TTarget> data);
}