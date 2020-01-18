namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingValues
    {
        public MappingValues(
            Expression sourceValue, 
            Expression targetValue, 
            Expression elementIndex, 
            Expression elementKey)
        {
            SourceValue = sourceValue;
            TargetValue = targetValue;
            ElementIndex = elementIndex;
            ElementKey = elementKey;
        }

        public Expression SourceValue { get; }

        public Expression TargetValue { get; }

        public Expression ElementIndex { get; }

        public Expression ElementKey { get; }
    }
}