namespace AgileObjects.AgileMapper.Members
{
    internal interface IQualifiedMemberContext : IMapperContextOwner, IRuleSetOwner, ITypePair
    {
        bool IsRoot { get; }

        bool IsEntryPoint { get; }

        IQualifiedMemberContext Parent { get; }

        IQualifiedMember SourceMember { get; }
        
        QualifiedMember TargetMember { get; }
    }
}