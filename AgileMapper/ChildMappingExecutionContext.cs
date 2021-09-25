namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal class ChildMappingExecutionContext<TChildSource, TChildTarget> :
        SubObjectMappingExecutionContextBase<TChildSource>
    {
        private readonly TChildSource _source;
        private readonly TChildTarget _target;
        private readonly int? _elementIndex;
        private readonly object _elementKey;
        private readonly ObjectMapperKeyBase _mapperKey;

        public ChildMappingExecutionContext(
            TChildSource source,
            TChildTarget target,
            int? elementIndex,
            object elementKey,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            MappingExecutionContextBase2 parent,
            MappingExecutionContextBase2 entryPointContext)
            : base(source, parent, entryPointContext)
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
                MappingContext = this
            };
        }

        public override ObjectMapperKeyBase GetMapperKey() => _mapperKey;

        public override object Target => _target;

        public override IObjectMappingData ToMappingData()
        {
            return ObjectMappingDataFactory.Create(
                _source,
                _target,
                _elementIndex,
                _elementKey,
                _mapperKey,
                GetParentMappingData());
        }
    }
}