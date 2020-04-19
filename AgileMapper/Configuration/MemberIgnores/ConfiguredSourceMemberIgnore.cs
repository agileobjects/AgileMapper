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

    internal class ConfiguredSourceMemberIgnore : ConfiguredSourceMemberIgnoreBase, IMemberIgnore
    {
#if NET35
        public ConfiguredSourceMemberIgnore(MappingConfigInfo configInfo, LinqExp.LambdaExpression sourceMemberLambda)
            : this(configInfo, sourceMemberLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredSourceMemberIgnore(MappingConfigInfo configInfo, LambdaExpression sourceMemberLambda)
            : base(configInfo)
        {
            SourceMember = sourceMemberLambda.ToSourceMemberOrNull(configInfo.MapperContext, out var failureReason) ??
                           throw new MappingConfigurationException(failureReason);
        }

        private ConfiguredSourceMemberIgnore(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo)
        {
            SourceMember = sourceMember;
        }

        public QualifiedMember SourceMember { get; }

        QualifiedMember IMemberIgnore.Member => SourceMember;

        protected override bool ConflictsWith(QualifiedMember sourceMember)
            => SourceMember.Matches(sourceMember);

        public override string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            return $"Configured data source {conflictingDataSource.GetDescription()} " +
                    "conflicts with an ignored source member";
        }

        public override string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore)
            => ((IMemberIgnore)this).GetConflictMessage(conflictingSourceMemberIgnore);

        public override bool AppliesTo(IQualifiedMemberContext context)
            => base.AppliesTo(context) && SourceMembersMatch(context.SourceMember as QualifiedMember);

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (!(otherItem is ConfiguredSourceMemberIgnore otherIgnoredMember))
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
            return new ConfiguredSourceMemberIgnore(ConfigInfo, SourceMember)
            {
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            if (!(autoCreatedItem is ConfiguredSourceMemberIgnore clonedIgnoredSourceMember))
            {
                return false;
            }

            return ConfigInfo.HasSameTypesAs(clonedIgnoredSourceMember) &&
                   MembersConflict(clonedIgnoredSourceMember);
        }

        #endregion
    }
}