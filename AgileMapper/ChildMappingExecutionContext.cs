namespace AgileObjects.AgileMapper
{
    using Members;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal class ChildMappingExecutionContext<TChildSource, TChildTarget> :
        SubObjectMappingExecutionContextBase,
        IMappingData<TChildSource, TChildTarget>
    {
        private readonly TChildSource _source;
        private readonly TChildTarget _target;
        private readonly int? _elementIndex;
        private readonly object _elementKey;
        private readonly ObjectMapperKeyBase _mapperKey;
        private IObjectMappingData _childMappingData;

        public ChildMappingExecutionContext(
            TChildSource source,
            TChildTarget target,
            int? elementIndex,
            object elementKey,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            MappingExecutionContextBase2 parent)
            : base(source, target, elementIndex, elementKey, parent)
        {
            _source = source;
            _target = target;
            _elementIndex = elementIndex;
            _elementKey = elementKey;

            _mapperKey = new ChildObjectMapperKey(
                MappingTypes.For(source, target),
                targetMemberRegistrationName,
                dataSourceIndex)
            {
                KeyData = this
            };
        }

        public override ObjectMapperKeyBase GetMapperKey() => _mapperKey;

        public override IObjectMappingData GetMappingData()
        {
            return _childMappingData ??= ObjectMappingDataFactory.Create(
                _source,
                _target,
                _elementIndex,
                _elementKey,
                _mapperKey,
                GetParentMappingData());
        }

        #region IMappingData<,> Members

        TChildSource IMappingData<TChildSource, TChildTarget>.Source => _source;

        TChildTarget IMappingData<TChildSource, TChildTarget>.Target => _target;

        #endregion
    }
}