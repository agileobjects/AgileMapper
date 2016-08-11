namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

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

        public bool SourceCanBeNull => _conditionBuilder.SourceCanBeNull;

        public IEnumerable<Expression> GetConditions(MemberMapperData data)
        {
            yield return _conditionBuilder.GetCondition(data);
        }
    }
}