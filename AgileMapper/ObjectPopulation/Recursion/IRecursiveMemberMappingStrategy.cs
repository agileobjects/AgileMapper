namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System.Linq.Expressions;

    internal interface IRecursiveMemberMappingStrategy
    {
        Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData);
    }
}
