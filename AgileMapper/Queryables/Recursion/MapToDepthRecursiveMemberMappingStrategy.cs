namespace AgileObjects.AgileMapper.Queryables.Recursion
{
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ObjectPopulation.Recursion;

    internal class MapToDepthRecursiveMemberMappingStrategy : IRecursiveMemberMappingStrategy
    {
        public Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            if (childMappingData.MapperData.TargetMember.IsComplex)
            {
                return Constants.EmptyExpression;
            }

            var helper = childMappingData.MapperData.EnumerablePopulationBuilder.TargetTypeHelper;

            return helper.GetEmptyInstanceCreation();
        }
    }
}