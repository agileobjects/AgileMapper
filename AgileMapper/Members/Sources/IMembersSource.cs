namespace AgileObjects.AgileMapper.Members.Sources
{
    internal interface IMembersSource
    {
        IQualifiedMember GetSourceMember<TSource, TTarget>();

        QualifiedMember GetTargetMember<TTarget>();
    }
}