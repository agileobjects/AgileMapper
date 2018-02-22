namespace AgileObjects.AgileMapper.Members.Sources
{
    internal interface IMembersSource
    {
        int DataSourceIndex { get; }

        IQualifiedMember GetSourceMember<TSource, TTarget>();

        QualifiedMember GetTargetMember<TSource, TTarget>();
    }
}