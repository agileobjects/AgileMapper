namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

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

        public Expression GetCondition(IMemberMappingContext context)
        {
            if (_nullSource && _nullExisting)
            {
                return Expression.AndAlso(
                    context.SourceObject.GetIsDefaultComparison(),
                    context.TargetObject.GetIsDefaultComparison());
            }

            if (_nullSource)
            {
                return context.SourceObject.GetIsDefaultComparison();
            }

            if (_nullExisting)
            {
                return context.TargetObject.GetIsDefaultComparison();
            }

            return Expression.Constant(false);
        }
    }
}