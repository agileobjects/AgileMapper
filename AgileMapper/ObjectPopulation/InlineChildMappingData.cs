namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class InlineChildMappingData<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
        IInlineMappingData
    {
        private readonly string _targetMemberRegistrationName;
        private readonly int _dataSourceIndex;
        private readonly IObjectMappingData _objectMappingDataParent;
        private readonly IInlineMappingData _inlineMappingDataParent;
        private readonly Func<ObjectMapperData> _mapperDataLoader;
        private ObjectMapperData _mapperData;

        public InlineChildMappingData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
            : this(source, target, enumerableIndex, targetMemberRegistrationName, dataSourceIndex, (IInlineMappingData)parent)
        {
            _objectMappingDataParent = parent;
            _mapperDataLoader = LoadMapperDataFromObjectMapperDataParent;
        }

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
            _inlineMappingDataParent = parent;
            _mapperDataLoader = LoadMapperDataFromInlineMapperDataParent;
        }

        public IMemberMapperData MapperData => _mapperData ?? (_mapperData = _mapperDataLoader.Invoke());

        private ObjectMapperData LoadMapperDataFromObjectMapperDataParent()
        {
            var childMapperData = _objectMappingDataParent.MapperData
                .GetChildMapperDataFor(_targetMemberRegistrationName, _dataSourceIndex);

            return childMapperData;
        }

        private ObjectMapperData LoadMapperDataFromInlineMapperDataParent()
        {
            var parentMapperData = (ObjectMapperData)_inlineMappingDataParent.MapperData;
            var childMapperData = parentMapperData.GetChildMapperDataFor(_targetMemberRegistrationName, _dataSourceIndex);

            return childMapperData;
        }

        public TTarget CreatedObject { get; set; }

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            => _inlineMappingDataParent.TryGet(key, out complexType);

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
            => _inlineMappingDataParent.Register(key, complexType);
    }
}