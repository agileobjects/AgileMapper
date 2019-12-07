namespace AgileObjects.AgileMapper.Members
{
    internal interface IBasicMapperData : IMapperContextOwner, IRuleSetOwner, ITypePair
    {
        bool IsRoot { get; }

        bool IsEntryPoint { get; }

        IBasicMapperData Parent { get; }

        IQualifiedMember SourceMember { get; }
        
        QualifiedMember TargetMember { get; }
    }
}