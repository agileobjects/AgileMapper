namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;

#endif

    internal interface IRepeatMappingStrategy
    {
        Expression GetMapRepeatedCallFor(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData);
    }
}
