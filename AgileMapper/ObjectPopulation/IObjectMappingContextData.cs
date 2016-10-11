namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal interface IObjectMappingContextData : IBasicMappingContextData
    {
        IMappingContext MappingContext { get; }

        bool RuntimeTypesAreTheSame { get; }

        ObjectMapperData MapperData { get; }

        IMemberMappingContextData GetChildContextData(MemberMapperData childMapperData);

        ObjectMapperKey MapperKeyObject { get; }

        void AddSourceMemberTypeTester(Func<IMappingData, bool> tester);
    }
}