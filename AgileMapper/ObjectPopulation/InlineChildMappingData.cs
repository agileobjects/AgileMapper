namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class InlineChildMappingData<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
        IInlineMappingData
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;
        private readonly IInlineMappingData _parent;
        private ObjectMapperData _mapperData;

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
            _parent = parent;
        }

        public IMemberMapperData MapperData => _mapperData ?? (_mapperData = LoadMapperData());

        private ObjectMapperData LoadMapperData()
        {
            var objectMappingDataParent = _parent as IObjectMappingData;

            var parentMapperData = (ObjectMapperData)((objectMappingDataParent != null)
                ? objectMappingDataParent.MapperData
                : _parent.MapperData);

            var childMapperData = parentMapperData.GetChildMapperDataFor(_targetMemberRegistrationName, _dataSourceIndex);

            return childMapperData;
        }

        public TTarget CreatedObject { get; set; }

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            => _parent.TryGet(key, out complexType);

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
            => _parent.Register(key, complexType);
    }
}