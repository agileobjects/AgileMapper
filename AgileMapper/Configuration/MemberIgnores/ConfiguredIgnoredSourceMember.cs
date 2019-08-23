namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
    using Extensions.Internal;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class ConfiguredIgnoredSourceMember : ConfiguredIgnoredSourceMemberBase
    {
#if NET35
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LinqExp.LambdaExpression sourceMemberLambda)
            : this(configInfo, sourceMemberLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LambdaExpression sourceMemberLambda)
            : base(configInfo)
        {
            SourceMember = sourceMemberLambda.ToSourceMemberOrNull(configInfo.MapperContext, out var failureReason) ??
                           throw new MappingConfigurationException(failureReason);
        }

        private ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo)
        {
            SourceMember = sourceMember;
        }

        public QualifiedMember SourceMember { get; }

        public override string GetConflictMessage(ConfiguredIgnoredSourceMemberBase conflictingIgnoredSourceMember)
        {
            if (conflictingIgnoredSourceMember is ConfiguredIgnoredSourceMemberFilter ignoredSourceMemberFilter)
            {
                return ignoredSourceMemberFilter.GetConflictMessage(this);
            }

            return $"Member {SourceMember.GetPath()} has already been ignored";
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
            => base.AppliesTo(mapperData) && SourceMembersMatch(mapperData.SourceMember as QualifiedMember);

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (!(otherItem is ConfiguredIgnoredSourceMember otherIgnoredMember))
            {
                return false;
            }

            return SourceMembersMatch(otherIgnoredMember.SourceMember);
        }

        private bool SourceMembersMatch(QualifiedMember otherSourceMember)
        {
            if ((SourceMember == null) || (otherSourceMember == null))
            {
                return false;
            }

            return SourceMember.LeafMember.Equals(otherSourceMember.LeafMember);
        }

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredIgnoredSourceMember(ConfigInfo, SourceMember)
            {
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            if (!(autoCreatedItem is ConfiguredIgnoredSourceMember clonedIgnoredSourceMember))
            {
                return false;
            }

            return ConfigInfo.HasSameSourceTypeAs(clonedIgnoredSourceMember.ConfigInfo) &&
                   ConfigInfo.HasSameTargetTypeAs(clonedIgnoredSourceMember.ConfigInfo) &&
                   MembersConflict(clonedIgnoredSourceMember);
        }

        #endregion
    }
}