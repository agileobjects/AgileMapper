namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal abstract class InlineElementMappingDataBase<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
        IInlineMappingData
    {
        private readonly IInlineMappingData _parent;
        private ObjectMapperData _mapperData;

        protected InlineElementMappingDataBase(
            TSource sourceElement,
            TTarget targetElement,
            int? enumerableIndex,
            IInlineMappingData parent)
            : base(sourceElement, targetElement, enumerableIndex, parent)
        {
            _parent = parent;
        }

        public IMemberMapperData MapperData => _mapperData ?? (_mapperData = LoadMapperData());

        private ObjectMapperData LoadMapperData()
        {
            var objectMappingDataParent = _parent as IObjectMappingData;

            var parentMapperData = (ObjectMapperData)((objectMappingDataParent != null)
                ? objectMappingDataParent.MapperData
                : _parent.MapperData);

            var childMapperData = GetChildMapperDataFrom(parentMapperData);

            return childMapperData;
        }

        protected abstract ObjectMapperData GetChildMapperDataFrom(ObjectMapperData parentMapperData);

        public TTarget CreatedObject { get; set; }

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            => _parent.TryGet(key, out complexType);

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
            => _parent.Register(key, complexType);
    }
}