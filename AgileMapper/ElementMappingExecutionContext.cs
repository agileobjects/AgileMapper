namespace AgileObjects.AgileMapper;

using Members;
using ObjectPopulation;
using ObjectPopulation.Enumerables;
using ObjectPopulation.MapperKeys;

internal class ElementMappingExecutionContext<TElementSource, TElementTarget> :
    SubObjectMappingExecutionContextBase,
    IMappingData<TElementSource, TElementTarget>
{
    private readonly TElementSource _sourceElement;
    private readonly TElementTarget _targetElement;
    private readonly int _elementIndex;
    private readonly object _elementKey;
    private readonly ObjectMapperKeyBase _mapperKey;
    private IObjectMappingData _elementMappingData;

    public ElementMappingExecutionContext(
        TElementSource sourceElement,
        TElementTarget targetElement,
        int elementIndex,
        object elementKey,
        MappingExecutionContextBase2 parent)
        : base(sourceElement, targetElement, elementIndex, elementKey, parent)
    {
        _sourceElement = sourceElement;
        _targetElement = targetElement;
        _elementIndex = elementIndex;
        _elementKey = elementKey;

        _mapperKey = new ElementObjectMapperKey(
            MappingTypes.For(sourceElement, targetElement))
        {
            KeyData = this
        };
    }

    public override ObjectMapperKeyBase GetMapperKey() => _mapperKey;

    public override IObjectMappingData GetMappingData()
    {
        return _elementMappingData ??= ObjectMappingDataFactory.Create(
            _sourceElement,
            _targetElement,
            _elementIndex,
            _elementKey,
            _mapperKey,
            GetParentMappingData());
    }

    #region IMappingData<,> Members

    TElementSource IMappingData<TElementSource, TElementTarget>.Source
        => _sourceElement;

    TElementTarget IMappingData<TElementSource, TElementTarget>.Target
        => _targetElement;

    #endregion
}