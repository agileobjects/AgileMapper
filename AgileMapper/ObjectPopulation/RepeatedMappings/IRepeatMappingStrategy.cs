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
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData);
    }
}
