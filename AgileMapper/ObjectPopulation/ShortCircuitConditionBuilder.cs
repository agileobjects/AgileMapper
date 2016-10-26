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

        public Expression GetConditionOrNull(IMemberMapperData mapperData)
        {
            if (_nullSource && _nullExisting)
            {
                return Expression.AndAlso(
                    mapperData.SourceObject.GetIsDefaultComparison(),
                    mapperData.TargetObject.GetIsDefaultComparison());
            }

            if (_nullSource)
            {
                // Root source is null-checked before mapping begins:
                return mapperData.IsRoot ? null : mapperData.SourceObject.GetIsDefaultComparison();
            }

            if (_nullExisting)
            {
                return mapperData.TargetObject.GetIsDefaultComparison();
            }

            return null;
        }
    }
}