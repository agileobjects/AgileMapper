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
#if NET35
    using LinqExp = System.Linq.Expressions;
#endif

    internal class ConfiguredFilterDataSourceFactory : ConfiguredDataSourceFactoryBase
    {
        private readonly Expression _targetMemberFilterExpression;
        private readonly Func<TargetMemberSelector, bool> _targetMemberFilter;
#if NET35
        public ConfiguredFilterDataSourceFactory(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<TargetMemberSelector, bool>> targetMemberFilter,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : this(
                configInfo,
                targetMemberFilter.ToDlrExpression(),
                dataSourceLambda,
                toTargetMember)
        {
        }
#endif

        public ConfiguredFilterDataSourceFactory(
            MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> targetMemberFilter,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : this(
                configInfo,
                targetMemberFilter?.Body,
                targetMemberFilter?.Compile() ?? (_ => true),
                dataSourceLambda,
                toTargetMember)
        {
        }

        public ConfiguredFilterDataSourceFactory(
            MappingConfigInfo configInfo,
            Expression targetMemberFilterExpression,
            Func<TargetMemberSelector, bool> targetMemberFilter,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember toTargetMember)
            : base(configInfo, dataSourceLambda, toTargetMember)
        {
            _targetMemberFilterExpression = targetMemberFilterExpression;
            _targetMemberFilter = targetMemberFilter;
        }

        protected override string GetConflictReasonOrNull(ConfiguredDataSourceFactoryBase conflictingDataSource)
            => null;

        protected override bool TargetMembersAreCompatibleForToTarget(QualifiedMember otherTargetMember)
            => MatchesTargetMember(otherTargetMember);

        private bool MatchesTargetMember(QualifiedMember targetMember)
            => _targetMemberFilter.Invoke(new TargetMemberSelector(targetMember));

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredFilterDataSourceFactory(
                ConfigInfo,
                _targetMemberFilterExpression,
                _targetMemberFilter,
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
