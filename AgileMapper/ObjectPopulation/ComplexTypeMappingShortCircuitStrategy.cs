namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class ComplexTypeMappingShortCircuitStrategy : IComplexTypeMappingShortCircuitStrategy
    {
        public static readonly IComplexTypeMappingShortCircuitStrategy SourceAndExistingAreNull =
            new ComplexTypeMappingShortCircuitStrategy(b => b.SourceIsNull().And.ExistingIsNull());

        public static readonly IComplexTypeMappingShortCircuitStrategy SourceIsNull =
            new ComplexTypeMappingShortCircuitStrategy(b => b.SourceIsNull());

        private readonly ShortCircuitConditionBuilder _conditionBuilder;

        public ComplexTypeMappingShortCircuitStrategy(Action<ShortCircuitConditionBuilder> conditionConfigurator)
        {
            _conditionBuilder = new ShortCircuitConditionBuilder();

            conditionConfigurator.Invoke(_conditionBuilder);
        }

        public IEnumerable<Expression> GetConditions(Expression sourceObject, IObjectMappingContext omc)
        {
            yield return _conditionBuilder.GetCondition(sourceObject, omc);
        }
    }
}