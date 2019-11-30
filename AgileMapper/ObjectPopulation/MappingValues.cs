namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingValues
    {
        public MappingValues(Expression sourceValue, Expression targetValue, Expression elementIndex)
        {
            SourceValue = sourceValue;
            TargetValue = targetValue;
            ElementIndex = elementIndex;
        }

        public Expression SourceValue { get; }

        public Expression TargetValue { get; }

        public Expression ElementIndex { get; }
    }
}