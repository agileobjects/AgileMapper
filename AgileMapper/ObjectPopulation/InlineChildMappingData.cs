namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class InlineChildMappingData<TSource, TTarget> :
        InlineElementMappingDataBase<TSource, TTarget>
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;

        public InlineChildMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IInlineMappingData parent)
            : base(source, target, enumerableIndex, parent)
        {
            _targetMemberRegistrationName = targetMemberRegistrationName;
            _dataSourceIndex = dataSourceIndex;
        }

        protected override ObjectMapperData GetChildMapperDataFrom(ObjectMapperData parentMapperData)
            => parentMapperData.GetChildMapperDataFor(_targetMemberRegistrationName, _dataSourceIndex);
    }
}