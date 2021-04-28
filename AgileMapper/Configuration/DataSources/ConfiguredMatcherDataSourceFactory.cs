namespace AgileObjects.AgileMapper.Configuration.DataSources
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using Extensions.Internal;
#else
    using System.Linq.Expressions;
#endif
    using Lambdas;
    using Members;
    using ReadableExpressions;
#if NET35
    using LinqExp = System.Linq.Expressions;
#endif

    internal class ConfiguredMatcherDataSourceFactory :
        ConfiguredDataSourceFactoryBase,
        IHasMemberFilter
    {
        private readonly Expression _targetMemberMatcherExpression;
        private readonly Func<TargetMemberSelector, bool> _targetMemberMatcher;
#if NET35
        public ConfiguredMatcherDataSourceFactory(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<TargetMemberSelector, bool>> targetMemberMatcher,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : this(
                configInfo,
                targetMemberMatcher.ToDlrExpression(),
                dataSourceLambda,
                toTargetMember)
        {
        }
#endif

        public ConfiguredMatcherDataSourceFactory(
            MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> targetMemberMatcher,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : this(
                configInfo,
                targetMemberMatcher?.Body,
                targetMemberMatcher?.Compile() ?? (_ => true),
                dataSourceLambda,
                toTargetMember)
        {
        }

        private ConfiguredMatcherDataSourceFactory(
            MappingConfigInfo configInfo,
            Expression targetMemberMatcherExpression,
            Func<TargetMemberSelector, bool> targetMemberMatcher,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : base(configInfo, dataSourceLambda, toTargetMember)
        {
            _targetMemberMatcherExpression = targetMemberMatcherExpression;
            _targetMemberMatcher = targetMemberMatcher;
        }

        private string TargetMemberMatcher => _targetMemberMatcherExpression?.ToReadableString();

        string IHasMemberFilter.MemberFilter => TargetMemberMatcher;

        #region ConflictsWith Helpers

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            if (otherItem is IHasMemberFilter memberFilterOwner)
            {
                return HasSameCriteriaAs(memberFilterOwner);
            }

            return MatcherMatches(otherItem.TargetMember);
        }

        protected override bool HasSameCriteriaAs(ConfiguredDataSourceFactoryBase otherDataSource)
        {
            return otherDataSource is ConfiguredMatcherDataSourceFactory matcherDataSource &&
                   HasSameCriteriaAs(matcherDataSource);
        }

        private bool HasSameCriteriaAs(IHasMemberFilter memberFilterOwner)
            => memberFilterOwner.MemberFilter == TargetMemberMatcher;

        #endregion

        protected override string GetToTargetDescription(ConfiguredDataSourceFactoryBase conflictingDataSource)
            => null;

        protected override string GetConflictReasonOrNull(ConfiguredDataSourceFactoryBase conflictingDataSource)
            => null;

        public override string GetDescription() => GetDataSourceDescription();

        protected override string GetDataSourceDescription()
        {
            var source = SourceType != typeof(object) ? SourceTypeName + " " : null;
            var members = TargetType != DataSourceLambda.ReturnType ? " members" : null;

            return
                $"'If mapping {source}-> {GetTargetDescription()} and {TargetMemberMatcher}, " +
                 $"map {GetDataSourceValueDescription()} to target{members}'";
        }

        protected override string GetTargetDescription() => TargetTypeName;

        protected override bool TargetMembersAreCompatibleForToTarget(QualifiedMember otherTargetMember)
            => MatcherMatches(otherTargetMember);

        private bool MatcherMatches(QualifiedMember targetMember)
            => _targetMemberMatcher.Invoke(new TargetMemberSelector(targetMember));

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredMatcherDataSourceFactory(
                ConfigInfo,
                _targetMemberMatcherExpression,
                _targetMemberMatcher,
                DataSourceLambda,
                TargetMember)
            {
                ValueCouldBeSourceMember = ValueCouldBeSourceMember,
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedDataSourceFactory)
            => false;

        #endregion
    }
}
