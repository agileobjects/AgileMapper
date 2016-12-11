namespace AgileObjects.AgileMapper.Members.Sources
{
    internal class RootMembersSource : IMembersSource
    {
        private readonly RootQualifiedMemberFactory _memberFactory;

        public RootMembersSource(RootQualifiedMemberFactory memberFactory)
        {
            _memberFactory = memberFactory;
        }

        public IQualifiedMember GetSourceMember<TSource, TTarget>() => _memberFactory.RootSource<TSource, TTarget>();

        public QualifiedMember GetTargetMember<TTarget>() => _memberFactory.RootTarget<TTarget>();
    }
}