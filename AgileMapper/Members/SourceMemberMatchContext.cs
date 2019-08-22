namespace AgileObjects.AgileMapper.Members
{
    using NetStandardPolyfills;

    internal class SourceMemberMatchContext
    {
        public SourceMemberMatchContext(
            IChildMemberMappingData memberMappingData,
            bool searchParentContexts = true)
        {
            MemberMappingData = memberMappingData;
            SearchParentContexts = searchParentContexts;
        }

        public IChildMemberMappingData MemberMappingData { get; private set; }

        public IMemberMapperData MemberMapperData => MemberMappingData.MapperData;

        public IQualifiedMember ParentSourceMember => MemberMapperData.SourceMember;

        public bool SearchParentContexts { get; }

        public IQualifiedMember MatchingSourceMember { get; set; }

        public SourceMemberMatch SourceMemberMatch { get; set; }

        public bool TypesAreCompatible
            => MemberMapperData.TargetType.IsAssignableTo(MemberMapperData.SourceType);

        public SourceMemberMatchContext With(IChildMemberMappingData memberMappingData)
        {
            MemberMappingData = memberMappingData;
            MatchingSourceMember = null;
            SourceMemberMatch = null;
            return this;
        }
    }
}