namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal class MappingValues
    {
        public MappingValues(Expression sourceValue, Expression targetValue, Expression enumerableIndex)
        {
            SourceValue = sourceValue;
            TargetValue = targetValue;
            EnumerableIndex = enumerableIndex;
        }

        public Expression SourceValue { get; }

        public Expression TargetValue { get; }

        public Expression EnumerableIndex { get; }
    }
}