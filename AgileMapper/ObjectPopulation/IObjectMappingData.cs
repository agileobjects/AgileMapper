namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;

    internal interface IObjectMappingData : IBasicMappingData, IInlineMappingData
    {
        IMappingContext MappingContext { get; }

        ElementMembersSource ElementMembersSource { get; }

        ObjectMapperKeyBase MapperKey { get; }

        new ObjectMapperData MapperData { get; }

        IObjectMapper Mapper { get; set; }

        IObjectMapper CreateMapper();

        IMemberMappingData GetChildMappingData(IMemberMapperData childMapperData);

        object MapStart();
    }
}