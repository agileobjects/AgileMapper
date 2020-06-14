namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
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

    internal class ConfiguredMemberIgnore : ConfiguredMemberIgnoreBase, IMemberIgnore
    {
#if NET35
        public ConfiguredMemberIgnore(MappingConfigInfo configInfo, LinqExp.LambdaExpression targetMemberLambda)
            : this(configInfo, targetMemberLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredMemberIgnore(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        private ConfiguredMemberIgnore(MappingConfigInfo configInfo, QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
        }

        public override string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            return $"Configured data source {conflictingDataSource.GetDescription()} " +
                    "conflicts with an ignored member";
        }

        public override string GetConflictMessage(ConfiguredMemberIgnoreBase conflictingMemberIgnore)
            => ((IMemberIgnore)this).GetConflictMessage(conflictingMemberIgnore);

        public override string GetIgnoreMessage(IQualifiedMember targetMember)
            => targetMember.Name + " is ignored";

        QualifiedMember IMemberIgnore.Member => TargetMember;

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredMemberIgnore(ConfigInfo, TargetMember)
            {
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            if (!(autoCreatedItem is ConfiguredMemberIgnore clonedIgnoredMember))
            {
                return false;
            }

            return ConfigInfo.HasSameTypesAs(clonedIgnoredMember) && MembersConflict(clonedIgnoredMember);
        }

        #endregion
    }
}