namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using DataSources.Factories;
#if NET35
    using Extensions.Internal;
#endif
    using Members;

    internal class ConfiguredMemberIgnore : ConfiguredMemberIgnoreBase
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

        public override string GetConflictMessage(ConfiguredMemberIgnoreBase conflictingMemberIgnore)
        {
            if (conflictingMemberIgnore is ConfiguredMemberFilterIgnore ignoredMemberFilter)
            {
                return ignoredMemberFilter.GetConflictMessage(this);
            }

            return $"Member {TargetMember.GetPath()} has already been ignored";
        }

        public override string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
            => $"Ignored member {TargetMember.GetPath()} has a configured data source";

        public override string GetIgnoreMessage(IQualifiedMember targetMember)
            => targetMember.Name + " is ignored";

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

            return ConfigInfo.HasSameSourceTypeAs(clonedIgnoredMember.ConfigInfo) &&
                   ConfigInfo.HasSameTargetTypeAs(clonedIgnoredMember.ConfigInfo) &&
                   MembersConflict(clonedIgnoredMember);
        }

        #endregion
    }
}