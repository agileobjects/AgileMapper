namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using System;
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
    using ReadableExpressions;

    internal class ConfiguredMemberFilter : ConfiguredMemberIgnoreBase, IMemberFilterIgnore
    {
        private readonly Expression _memberFilterExpression;
        private readonly Func<TargetMemberSelector, bool> _memberFilter;
#if NET35
        public ConfiguredMemberFilter(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredMemberFilter(
            MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.Body, memberFilterLambda.Compile())
        {
        }

        private ConfiguredMemberFilter(
            MappingConfigInfo configInfo,
            Expression memberFilterExpression,
            Func<TargetMemberSelector, bool> memberFilter)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterExpression;
            _memberFilter = memberFilter;
        }

        private string TargetMemberFilter => _memberFilterExpression?.ToReadableString();

        string IMemberFilterIgnore.MemberFilter => TargetMemberFilter;

        public override string GetConflictMessage(ConfiguredMemberIgnoreBase conflictingMemberIgnore)
            => ((IMemberFilterIgnore)this).GetConflictMessage(conflictingMemberIgnore);

        public string GetConflictMessage(ConfiguredMemberIgnore conflictingMemberIgnore)
            => ((IMemberFilterIgnore)this).GetConflictMessage(conflictingMemberIgnore);

        public override string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
            => $"Member ignore pattern '{TargetMemberFilter}' conflicts with a configured data source";

        public override string GetIgnoreMessage(IQualifiedMember targetMember)
            => $"{targetMember.Name} is ignored by filter:{Environment.NewLine}{TargetMemberFilter}";

        public override bool AppliesTo(IQualifiedMemberContext context)
            => base.AppliesTo(context) && IsFiltered(context.TargetMember);

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is ConfiguredMemberFilter otherIgnoredMemberFilter)
            {
                return otherIgnoredMemberFilter.TargetMemberFilter == TargetMemberFilter;
            }

            return IsFiltered(otherItem.TargetMember);
        }

        public bool IsFiltered(QualifiedMember member)
            => _memberFilter.Invoke(new TargetMemberSelector(member));

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredMemberFilter(
                ConfigInfo,
                _memberFilterExpression,
                _memberFilter)
            {
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem) => false;

        #endregion
    }
}