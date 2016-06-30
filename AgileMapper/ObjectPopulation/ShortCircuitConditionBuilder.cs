namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions;

    internal class ShortCircuitConditionBuilder
    {
        private bool _nullSource;
        private bool _nullExisting;

        public ShortCircuitConditionBuilder SourceIsNull()
        {
            _nullSource = true;
            return this;
        }

        public ShortCircuitConditionBuilder And => this;

        public ShortCircuitConditionBuilder ExistingIsNull()
        {
            _nullExisting = true;
            return this;
        }

        public Expression GetCondition(Expression sourceObject, IObjectMappingContext omc)
        {
            if (_nullSource && _nullExisting)
            {
                return Expression.AndAlso(
                    sourceObject.GetIsDefaultComparison(),
                    omc.TargetObject.GetIsDefaultComparison());
            }

            if (_nullSource)
            {
                return sourceObject.GetIsDefaultComparison();
            }

            if (_nullExisting)
            {
                return omc.TargetObject.GetIsDefaultComparison();
            }

            return Expression.Constant(false);
        }
    }
}