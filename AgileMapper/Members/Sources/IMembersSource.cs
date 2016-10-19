namespace AgileObjects.AgileMapper.Members.Sources
{
    internal interface IMembersSource
    {
        IQualifiedMember GetSourceMember<TSource>();

        QualifiedMember GetTargetMember<TTarget>();
    }
}