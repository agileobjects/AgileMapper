namespace AgileObjects.AgileMapper.Members.Sources
{
    internal class RootMembersSource : IMembersSource
    {
        private readonly QualifiedMemberFactory _memberFactory;

        public RootMembersSource(QualifiedMemberFactory memberFactory)
        {
            _memberFactory = memberFactory;
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public int DataSourceIndex => 0;

        public IQualifiedMember GetSourceMember<TSource, TTarget>()
            => _memberFactory.RootSource<TSource, TTarget>();

        public QualifiedMember GetTargetMember<TSource, TTarget>()
            => _memberFactory.RootTarget<TSource, TTarget>();
    }
}