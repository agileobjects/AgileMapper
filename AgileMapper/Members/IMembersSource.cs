namespace AgileObjects.AgileMapper.Members
{
    internal interface IMembersSource
    {
        IQualifiedMember GetSourceMember<TSource>();

        QualifiedMember GetTargetMember<TTarget>();
    }
}