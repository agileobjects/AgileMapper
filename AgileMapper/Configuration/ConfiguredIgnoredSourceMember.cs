namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
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

        private ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo)
        {
            _sourceMember = sourceMember;
        }

        public string GetConflictMessage(ConfiguredIgnoredSourceMember conflictingIgnoredSourceMember)
        {
            return $"Member {_sourceMember.GetPath()} has already been ignored";
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return base.AppliesTo(mapperData) &&
                   _sourceMember.LeafMember.Equals((mapperData.SourceMember as QualifiedMember)?.LeafMember);
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

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            return (otherItem is ConfiguredIgnoredSourceMember otherIgnoredSourceMember) &&
                   _sourceMember.LeafMember.Equals(otherIgnoredSourceMember._sourceMember.LeafMember);
        }

        #endregion

#if NET35
        int IComparable<ConfiguredIgnoredSourceMember>.CompareTo(ConfiguredIgnoredSourceMember other)
            => DoComparisonTo(other);
#endif
    }
}