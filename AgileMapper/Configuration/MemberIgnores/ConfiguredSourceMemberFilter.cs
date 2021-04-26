namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
#if NET35
    using Extensions.Internal;
#endif
    using Members;
    using ReadableExpressions;
#if NET35
    using LinqExp = System.Linq.Expressions;
#endif

    internal class ConfiguredSourceMemberFilter : ConfiguredSourceMemberIgnoreBase, IMemberFilterIgnore
    {
        private readonly Expression _memberFilterExpression;
        private readonly Func<SourceMemberSelector, bool> _memberFilter;
#if NET35
        public ConfiguredSourceMemberFilter(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<SourceMemberSelector, bool>> memberFilterLambda)
            : this(configInfo, memberFilterLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredSourceMemberFilter(
            MappingConfigInfo configInfo,
            Expression<Func<SourceMemberSelector, bool>> memberFilterLambda)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterLambda.Body;
            _memberFilter = memberFilterLambda.Compile();
        }

        private ConfiguredSourceMemberFilter(
            MappingConfigInfo configInfo,
            Expression memberFilterExpression,
            Func<SourceMemberSelector, bool> memberFilter)
            : base(configInfo)
        {
            _memberFilterExpression = memberFilterExpression;
            _memberFilter = memberFilter;
        }

        private string SourceMemberFilter => _memberFilterExpression?.ToReadableString();

        string IMemberFilterIgnore.MemberFilter => SourceMemberFilter;

        protected override bool ConflictsWith(QualifiedMember sourceMember) => IsFiltered(sourceMember);

        public override string GetConflictMessage(ConfiguredDataSourceFactoryBase conflictingDataSource)
        {
            return $"Configured data source {conflictingDataSource.GetDescription()} " +
                   $"conflicts with source member ignore pattern '{SourceMemberFilter}'";
        }

        public override string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore)
            => ((IMemberFilterIgnore)this).GetConflictMessage(conflictingSourceMemberIgnore);

        public override bool AppliesTo(IQualifiedMemberContext context)
        {
            return base.AppliesTo(context) &&
                  (context.SourceMember is QualifiedMember sourceMember) &&
                   IsFiltered(sourceMember);
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is ConfiguredSourceMemberFilter otherIgnoredMemberFilter)
            {
                return SourceMemberFilter == otherIgnoredMemberFilter.SourceMemberFilter;
            }

            if (otherItem is ConfiguredSourceMemberIgnore otherIgnoredMember)
            {
                return IsFiltered(otherIgnoredMember.SourceMember);
            }

            return false;
        }

        private bool IsFiltered(QualifiedMember sourceMember)
            => _memberFilter.Invoke(new SourceMemberSelector(sourceMember));

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredSourceMemberFilter(
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