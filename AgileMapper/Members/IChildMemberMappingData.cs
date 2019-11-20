namespace AgileObjects.AgileMapper.Members
{
    using ObjectPopulation;

    internal interface IChildMemberMappingData : IRuleSetOwner
    {
        IObjectMappingData Parent { get; }

        IMemberMapperData MapperData { get; }
    }
}