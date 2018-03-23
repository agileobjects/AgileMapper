namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    internal delegate TTarget MapperFunc<TSource, TTarget>(
        ObjectMappingData<TSource, TTarget> mappingData,
        ObjectCache mappedObjectCache,
        IRepeatedMappingFuncSet mappingFuncs);
}