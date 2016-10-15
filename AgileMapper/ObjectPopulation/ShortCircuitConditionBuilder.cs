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

        public Expression GetConditionOrNull(MemberMapperData mapperData)
        {
            if (_nullSource && _nullExisting)
            {
                return Expression.AndAlso(
                    mapperData.SourceObject.GetIsDefaultComparison(),
                    mapperData.TargetObject.GetIsDefaultComparison());
            }

            // Root source is null-checked before mapping begins:
            if (_nullSource)
            {
                return (mapperData.Parent != null) ?
                    mapperData.SourceObject.GetIsDefaultComparison()
                    : null;
            }

            if (_nullExisting)
            {
                return mapperData.TargetObject.GetIsDefaultComparison();
            }

            return null;
        }
    }
}