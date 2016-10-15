namespace AgileObjects.AgileMapper.Members
{
    internal class RootMembersSource : IMembersSource
    {
        private readonly RootQualifiedMemberFactory _memberFactory;

        public RootMembersSource(RootQualifiedMemberFactory memberFactory)
        {
            _memberFactory = memberFactory;
        }

        public IQualifiedMember GetSourceMember<TSource>() => _memberFactory.RootSource<TSource>();

        public QualifiedMember GetTargetMember<TTarget>() => _memberFactory.RootTarget<TTarget>();
    }
}