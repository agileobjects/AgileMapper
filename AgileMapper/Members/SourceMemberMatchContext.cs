namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class SourceMemberMatchContext
    {
        private IList<ConfiguredIgnoredSourceMember> _relevantSourceMemberIgnores;
        private IQualifiedMember _parentSourceMember;

        public SourceMemberMatchContext(
            IChildMemberMappingData memberMappingData,
            bool searchParentContexts = true)
        {
            MemberMappingData = memberMappingData;
            SearchParentContexts = searchParentContexts;
        }

        private UserConfigurationSet UserConfigurations => MemberMapperData.MapperContext.UserConfigurations;

        public IChildMemberMappingData MemberMappingData { get; private set; }

        public IMemberMapperData MemberMapperData => MemberMappingData.MapperData;

        public QualifiedMember TargetMember => MemberMapperData.TargetMember;

        public IQualifiedMember ParentSourceMember => _parentSourceMember ?? MemberMapperData.SourceMember;

        public bool SearchParentContexts { get; }

        public IQualifiedMember MatchingSourceMember { get; set; }

        public SourceMemberMatch SourceMemberMatch { get; set; }

        public bool TypesAreCompatible
            => MemberMapperData.TargetType.IsAssignableTo(MemberMapperData.SourceType);

        public bool HasSourceMemberIgnores => RelevantSourceMemberIgnores.Any();

        private IList<ConfiguredIgnoredSourceMember> RelevantSourceMemberIgnores
            => _relevantSourceMemberIgnores ??
              (_relevantSourceMemberIgnores = UserConfigurations.GetRelevantSourceMemberIgnores(MemberMapperData));

        public ConfiguredIgnoredSourceMember GetSourceMemberIgnoreOrNull(IQualifiedMember sourceMember)
            => RelevantSourceMemberIgnores.FindMatch(new BasicMapperData(sourceMember, TargetMember, MemberMapperData));

        public SourceMemberMatch CreateSourceMemberMatch(bool isUseable = true)
            => new SourceMemberMatch(MatchingSourceMember, MemberMappingData, isUseable);

        public SourceMemberMatchContext With(IQualifiedMember parentSourceMember)
        {
            _parentSourceMember = parentSourceMember;
            return this;
        }

        public SourceMemberMatchContext With(IChildMemberMappingData memberMappingData)
        {
            MemberMappingData = memberMappingData;
            _parentSourceMember = null;
            MatchingSourceMember = null;
            SourceMemberMatch = null;
            return this;
        }
    }
}