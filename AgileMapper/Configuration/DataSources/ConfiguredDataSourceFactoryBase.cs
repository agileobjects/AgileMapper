namespace AgileObjects.AgileMapper.Configuration.DataSources
{
#if NET35
    using System;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using AgileMapper.DataSources;
    using Api.Configuration;
    using Lambdas;
    using Members;
    using Members.Extensions;

    internal abstract class ConfiguredDataSourceFactoryBase :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredDataSourceFactoryBase>
#endif
    {
        protected ConfiguredDataSourceFactoryBase(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
            DataSourceLambda = dataSourceLambda;
        }

        protected ConfiguredDataSourceFactoryBase(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            LambdaExpression targetMemberLambda,
            bool valueCouldBeSourceMember)
            : base(configInfo, targetMemberLambda)
        {
            DataSourceLambda = dataSourceLambda;
            ValueCouldBeSourceMember = valueCouldBeSourceMember;
        }

        public bool IsForToTargetDataSource => TargetMember.IsRoot;

        public bool IsSequential => ConfigInfo.IsSequentialConfiguration;

        internal ConfiguredLambdaInfo DataSourceLambda { get; }

        protected bool ValueCouldBeSourceMember { get; set; }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherDataSource = otherConfiguredItem as ConfiguredDataSourceFactoryBase;
            var isOtherDataSource = otherDataSource != null;
            var criteriaAreTheSame = HasSameCriteriaAs(otherDataSource);

            if (WasAutoCreated &&
               (otherConfiguredItem is IPotentialAutoCreatedItem otherItem) &&
               !otherItem.WasAutoCreated)
            {
                return isOtherDataSource && criteriaAreTheSame;
            }

            if (isOtherDataSource == false)
            {
                return true;
            }

            if (otherDataSource.IsSequential || !ConfigInfo.HasSameTypesAs(otherDataSource))
            {
                return criteriaAreTheSame;
            }

            return true;
        }

        #region ConflictsWith Helpers

        protected abstract bool HasSameCriteriaAs(ConfiguredDataSourceFactoryBase otherDataSource);

        #endregion

        public string GetConflictMessage(ConfiguredDataSourceFactoryBase conflictingDataSource)
        {
            var toTarget = GetToTargetDescription(conflictingDataSource);
            var existingDataSource = conflictingDataSource.GetDataSourceDescription();
            var reason = GetConflictReasonOrNull(conflictingDataSource);

            return $"{GetTargetDescription()} already has configured {toTarget}data source {existingDataSource}{reason}";
        }

        protected abstract string GetToTargetDescription(ConfiguredDataSourceFactoryBase conflictingDataSource);

        protected abstract string GetConflictReasonOrNull(ConfiguredDataSourceFactoryBase conflictingDataSource);

        public abstract string GetDescription();

        protected abstract string GetDataSourceDescription();

        protected string GetDataSourceValueDescription()
        {
            var description = DataSourceLambda.GetDescription(ConfigInfo);

            return DataSourceLambda.IsSourceMember ? description : "'" + description + "'";
        }

        protected abstract string GetTargetDescription();

        public override bool AppliesTo(IQualifiedMemberContext context)
            => base.AppliesTo(context) && DataSourceLambda.Supports(context.RuleSet);

        protected override bool TargetMembersAreCompatible(QualifiedMember otherTargetMember)
        {
            if (base.TargetMembersAreCompatible(otherTargetMember))
            {
                return true;
            }

            return TargetMember.IsRoot && TargetMembersAreCompatibleForToTarget(otherTargetMember);
        }

        protected abstract bool TargetMembersAreCompatibleForToTarget(QualifiedMember otherTargetMember);

        public IConfiguredDataSource Create(IMemberMapperData mapperData)
        {
            var configuredCondition = GetConditionOrNull(mapperData);
            var value = DataSourceLambda.GetBody(mapperData);

            return new ConfiguredDataSource(
                configuredCondition,
                value,
                ConfigInfo.IsSequentialConfiguration,
                ConfigInfo.HasTargetMemberMatcher(),
                mapperData);
        }

        public QualifiedMember ToSourceMemberOrNull()
        {
            if (ValueCouldBeSourceMember &&
                DataSourceLambda.TryGetSourceMember(out var sourceMemberLambda))
            {
                return sourceMemberLambda.ToSourceMemberOrNull(ConfigInfo.MapperContext);
            }

            return null;
        }

        protected override int? GetSameTypesOrder(UserConfiguredItemBase other)
            => ((ConfiguredDataSourceFactoryBase)other).IsSequential ? -1 : base.GetSameTypesOrder(other);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; protected set; }

        public abstract IPotentialAutoCreatedItem Clone();

        public abstract bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedDataSourceFactory);

        #endregion

#if NET35
        int IComparable<ConfiguredDataSourceFactoryBase>.CompareTo(ConfiguredDataSourceFactoryBase other)
            => DoComparisonTo(other);
#endif
    }
}