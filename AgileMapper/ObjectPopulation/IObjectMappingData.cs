namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;
    using Members.Sources;

    internal interface IObjectMappingData : IMappingData
    {
        IMappingContext MappingContext { get; }

        MappingRuleSet RuleSet { get; }

        bool IsRoot { get; }

        new IObjectMappingData Parent { get; }

        Type SourceType { get; }

        Type TargetType { get; }

        ElementMembersSource ElementMembersSource { get; }

        ObjectMapperKeyBase MapperKey { get; }

        ObjectMapperData MapperData { get; set; }

        IObjectMapper Mapper { get; set; }

        IMemberMappingData GetChildMappingData(IMemberMapperData childMapperData);

        object MapStart();

        bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType);

        void Register<TKey, TComplex>(TKey key, TComplex complexType);
    }
}