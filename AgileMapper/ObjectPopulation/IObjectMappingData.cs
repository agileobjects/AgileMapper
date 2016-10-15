namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal interface IObjectMappingData : IBasicMappingData
    {
        IMappingContext MappingContext { get; }

        ElementMembersSource ElementMembersSource { get; }

        ObjectMapperKeyBase MapperKey { get; }

        new ObjectMapperData MapperData { get; }

        IObjectMapper Mapper { get; set; }

        IObjectMapper CreateMapper();

        IMemberMappingData GetChildMappingData(MemberMapperData childMapperData);

        object MapStart();

        bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType);

        void Register<TKey, TComplex>(TKey key, TComplex complexType);
    }
}