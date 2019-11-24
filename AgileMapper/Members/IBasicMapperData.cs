namespace AgileObjects.AgileMapper.Members
{
    internal interface IBasicMapperData : ITypePair, IRuleSetOwner, IMapperContextOwner
    {
        bool IsRoot { get; }

        bool IsEntryPoint { get; }

        IBasicMapperData Parent { get; }

        IQualifiedMember SourceMember { get; }
        
        QualifiedMember TargetMember { get; }
    }
}