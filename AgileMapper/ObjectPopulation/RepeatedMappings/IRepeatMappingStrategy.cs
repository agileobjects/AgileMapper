namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IRepeatMappingStrategy
    {
        bool AppliesTo(IBasicMapperData mapperData);

        bool WillNotMap(IBasicMapperData mapperData);

        Expression GetMapRepeatedCallFor(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData);
    }
}
