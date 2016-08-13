namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal interface IObjectMapperCreationData : IMapperCreationData
    {
        ObjectMapperData MapperData { get; }

        IMemberMapperCreationData GetChildCreationData(MemberMapperData childMapperData);
    }
}