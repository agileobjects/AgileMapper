namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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