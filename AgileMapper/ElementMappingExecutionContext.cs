namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.MapperKeys;

    internal class ElementMappingExecutionContext<TElementSource, TElementTarget> :
        SubObjectMappingExecutionContextBase<TElementSource>
    {
        private readonly TElementSource _sourceElement;
        private readonly TElementTarget _targetElement;
        private readonly int _elementIndex;
        private readonly object _elementKey;
        private readonly ObjectMapperKeyBase _mapperKey;

        public ElementMappingExecutionContext(
            TElementSource sourceElement,
            TElementTarget targetElement,
            int elementIndex,
            object elementKey,
            MappingExecutionContextBase2 parent,
            MappingExecutionContextBase2 entryPointContext)
            : base(sourceElement, parent, entryPointContext)
        {
            _sourceElement = sourceElement;
            _targetElement = targetElement;
            _elementIndex = elementIndex;
            _elementKey = elementKey;

            _mapperKey = new ElementObjectMapperKey(
                MappingTypes.For(sourceElement, targetElement))
            {
                MappingContext = this
            };
        }

        public override ObjectMapperKeyBase GetMapperKey() => _mapperKey;

        public override object Target => _targetElement;

        public override IObjectMappingData ToMappingData()
        {
            return ObjectMappingDataFactory.Create(
                _sourceElement,
                _targetElement,
                _elementIndex,
                _elementKey,
                _mapperKey,
                GetParentMappingData());
        }
    }
}