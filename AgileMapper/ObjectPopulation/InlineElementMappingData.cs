namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class InlineElementMappingData<TSourceElement, TTargetElement> :
        InlineElementMappingDataBase<TSourceElement, TTargetElement>
    {
        public InlineElementMappingData(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IInlineMappingData parent)
            : base(sourceElement, targetElement, enumerableIndex, parent)
        {
        }

        protected override ObjectMapperData GetChildMapperDataFrom(ObjectMapperData parentMapperData)
            => parentMapperData.GetElementMapperData();
    }
}