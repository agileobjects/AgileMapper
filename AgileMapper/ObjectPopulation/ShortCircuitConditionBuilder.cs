namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions;

    internal class ShortCircuitConditionBuilder
    {
        private bool _nullSource;
        private bool _nullExisting;

        public bool SourceCanBeNull => !_nullSource || (_nullSource && _nullExisting);

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

        public Expression GetCondition(IObjectMappingContext omc)
        {
            if (_nullSource && _nullExisting)
            {
                return Expression.AndAlso(
                    omc.SourceObject.GetIsDefaultComparison(),
                    omc.TargetObject.GetIsDefaultComparison());
            }

            if (_nullSource)
            {
                return omc.SourceObject.GetIsDefaultComparison();
            }

            if (_nullExisting)
            {
                return omc.TargetObject.GetIsDefaultComparison();
            }

            return Expression.Constant(false);
        }
    }
}