namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IRecursiveMemberMappingStrategy
    {
        Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData);
    }
}
