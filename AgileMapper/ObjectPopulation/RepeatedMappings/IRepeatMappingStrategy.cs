namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

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
