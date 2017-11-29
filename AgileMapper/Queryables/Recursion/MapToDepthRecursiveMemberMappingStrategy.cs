namespace AgileObjects.AgileMapper.Queryables.Recursion
{
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.Recursion;

    internal class MapToDepthRecursiveMemberMappingStrategy : IRecursiveMemberMappingStrategy
    {
        public Expression GetMapRecursionCallFor(
            IObjectMappingData childMappingData,
            Expression sourceValue,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var targetMember = childMappingData.MapperData.TargetMember;

            if (targetMember.IsComplex)
            {
                return Constants.EmptyExpression;
            }

            var helper = new EnumerableTypeHelper(targetMember);

            var emptyCollection = helper.IsList
                ? Expression.ListInit(Expression.New(helper.ListType), Enumerable<Expression>.EmptyArray)
                : (Expression)Expression.NewArrayInit(targetMember.ElementType);

            return helper.GetEnumerableConversion(emptyCollection, allowEnumerableAssignment: true);
        }
    }
}