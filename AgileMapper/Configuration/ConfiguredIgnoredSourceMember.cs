namespace AgileObjects.AgileMapper.Configuration
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
#if NET35
    using Extensions.Internal;
#endif
    using Members;

    internal class ConfiguredIgnoredSourceMember :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredIgnoredSourceMember>
#endif
    {
        private readonly Expression _memberFilterExpression;
        private readonly Func<SourceMemberSelector, bool> _memberFilter;
        private readonly QualifiedMember _sourceMember;

#if NET35
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LinqExp.LambdaExpression sourceMemberLambda)
            : this(configInfo, sourceMemberLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LambdaExpression sourceMemberLambda)
            : base(configInfo)
        {
            _sourceMember = sourceMemberLambda
                                .ToSourceMemberOrNull(configInfo.MapperContext, out var failureReason) ??
                            throw new MappingConfigurationException(failureReason);
        }

#if NET35
        public ConfiguredIgnoredSourceMember(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredIgnoredSourceMember(
            MappingConfigInfo configInfo,
            Expression<Func<SourceMemberSelector, bool>> memberFilterLambda)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterLambda.Body;
            _memberFilter = memberFilterLambda.Compile();
        }

        private ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo)
        {
            _sourceMember = sourceMember;
        }

        public string GetConflictMessage(ConfiguredIgnoredSourceMember conflictingIgnoredSourceMember)
        {
            return $"Member {_sourceMember.GetPath()} has already been ignored";
        }

        private bool HasMemberFilter => _memberFilter != null;

        private bool HasNoMemberFilter => !HasMemberFilter;

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            var sourceMember = mapperData.SourceMember as QualifiedMember;

            if (HasNoMemberFilter)
            {
                return SourceMembersMatch(sourceMember);
            }

            return (sourceMember != null) &&
                   _memberFilter.Invoke(new SourceMemberSelector(sourceMember));
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            return (otherItem is ConfiguredIgnoredSourceMember otherIgnoredSourceMember) &&
                    SourceMembersMatch(otherIgnoredSourceMember._sourceMember);
        }

        private bool SourceMembersMatch(QualifiedMember otherSourceMember)
        {
            if ((_sourceMember == null) || (otherSourceMember == null))
            {
                return false;
            }

            return _sourceMember.LeafMember.Equals(otherSourceMember.LeafMember);
        }

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; private set; }

        public IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredIgnoredSourceMember(ConfigInfo, _sourceMember)
            {
                WasAutoCreated = true
            };
        }

        public bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            var clonedIgnoredSourceMember = (ConfiguredIgnoredSourceMember)autoCreatedItem;

            return ConfigInfo.HasSameSourceTypeAs(clonedIgnoredSourceMember.ConfigInfo) &&
                   ConfigInfo.HasSameTargetTypeAs(clonedIgnoredSourceMember.ConfigInfo) &&
                   MembersConflict(clonedIgnoredSourceMember);
        }

        #endregion

#if NET35
        int IComparable<ConfiguredIgnoredSourceMember>.CompareTo(ConfiguredIgnoredSourceMember other)
            => DoComparisonTo(other);
#endif
    }
}