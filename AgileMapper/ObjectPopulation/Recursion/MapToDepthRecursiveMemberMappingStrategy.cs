namespace AgileObjects.AgileMapper.ObjectPopulation.Recursion
{
    using System.Linq.Expressions;
    using Enumerables;
    using Extensions;

    internal class MapToDepthRecursiveMemberMappingStrategy : IRecursiveMemberMappingStrategy
    {
        public Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = childMappingData.MapperData;

            if (childMapperData.TargetMember.IsComplex)
            {
                return Constants.EmptyExpression;
            }

            var emptyArray = Expression.NewArrayBounds(
                childMapperData.TargetMember.ElementType,
                0.ToConstantExpression());

            var helper = new EnumerableTypeHelper(childMapperData.TargetMember);

            return helper.GetEnumerableConversion(
                emptyArray,
                childMapperData.RuleSet.Settings.AllowEnumerableAssignment);
        }
    }
}