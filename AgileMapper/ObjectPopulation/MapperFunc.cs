namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal delegate TTarget MapperFunc<in TSource, TTarget>(IMappingData<TSource, TTarget> data);
}