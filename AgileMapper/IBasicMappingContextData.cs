namespace AgileObjects.AgileMapper
{
    using Members;

    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    internal interface IBasicMappingContextData : IMappingData, IBasicMapperData
    {
        IQualifiedMember SourceMember { get; }
    }
}