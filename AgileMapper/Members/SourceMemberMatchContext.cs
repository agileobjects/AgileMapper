namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using AgileMapper.Extensions.Internal;
    using Configuration;
    using Configuration.MemberIgnores;
    using NetStandardPolyfills;

    internal class SourceMemberMatchContext
    {
        private IList<ConfiguredSourceMemberIgnoreBase> _relevantSourceMemberIgnores;
        private IQualifiedMember _parentSourceMember;

        public SourceMemberMatchContext(
            IChildMemberMappingData memberMappingData,
            bool searchParentContexts = true)
        {
            MemberMappingData = memberMappingData;
            SearchParentContexts = searchParentContexts;
        }

        private MapperContext MapperContext => MemberMapperData.MapperContext;

        private UserConfigurationSet UserConfigurations => MapperContext.UserConfigurations;

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

        private IList<ConfiguredSourceMemberIgnoreBase> RelevantSourceMemberIgnores
            => _relevantSourceMemberIgnores ??= UserConfigurations.GetRelevantSourceMemberIgnores(MemberMapperData);

        public ConfiguredSourceMemberIgnoreBase GetSourceMemberIgnoreOrNull(IQualifiedMember sourceMember)
            => RelevantSourceMemberIgnores.FindMatch(new QualifiedMemberContext(sourceMember, TargetMember, MemberMapperData));

        public SourceMemberMatch CreateSourceMemberMatch(IQualifiedMember matchingSourceMember = null, bool isUseable = true)
        {
            if (matchingSourceMember == null)
            {
                matchingSourceMember = MatchingSourceMember;
            }

            var ignoreCondition = GetSourceMemberCondition(matchingSourceMember);

            matchingSourceMember = MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(matchingSourceMember, TargetMember);

            return new SourceMemberMatch(
                matchingSourceMember,
                MemberMappingData,
                ignoreCondition,
                isUseable);
        }

        private Expression GetSourceMemberCondition(IQualifiedMember sourceMember)
        {
            if (!HasSourceMemberIgnores)
            {
                return null;
            }

            var matchingIgnore = GetSourceMemberIgnoreOrNull(sourceMember);

            return (matchingIgnore?.HasConfiguredCondition == true)
                ? matchingIgnore.GetConditionOrNull(MemberMapperData)
                : null;
        }

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