namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using RepeatedMappings;

    internal interface IObjectMapper : IObjectMapperFunc
    {
        ObjectMapperData MapperData { get; }

        IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs { get; }

        void CacheRepeatedMappingFuncs();

        bool IsStaticallyCacheable();

        void Reset();
    }
}