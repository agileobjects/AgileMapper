namespace AgileObjects.AgileMapper.Configuration.DataSources
{
#if NET35
    using System;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using AgileMapper.DataSources;
    using Lambdas;
    using Members;

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

        protected ConfiguredLambdaInfo DataSourceLambda { get; }

        protected bool ValueCouldBeSourceMember { get; set; }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherDataSource = otherConfiguredItem as ConfiguredDataSourceFactoryBase;
            var isOtherDataSource = otherDataSource != null;
            var dataSourceLambdasAreTheSame = HasSameDataSourceAs(otherDataSource);

            if (WasAutoCreated &&
               (otherConfiguredItem is IPotentialAutoCreatedItem otherItem) &&
               !otherItem.WasAutoCreated)
            {
                return isOtherDataSource && dataSourceLambdasAreTheSame;
            }

            if (isOtherDataSource == false)
            {
                return true;
            }

            if (!ConfigInfo.HasSameTypesAs(otherDataSource))
            {
                return dataSourceLambdasAreTheSame;
            }

            if (otherDataSource.IsSequential)
            {
                return dataSourceLambdasAreTheSame;
            }

            return true;
        }

        #region ConflictsWith Helpers

        private bool HasSameDataSourceAs(ConfiguredDataSourceFactoryBase otherDataSource)
            => DataSourceLambda.IsSameAs(otherDataSource?.DataSourceLambda);

        #endregion

        public string GetConflictMessage(ConfiguredDataSourceFactoryBase conflictingDataSource)
        {
            var toTarget = TargetMember.IsRoot
                ? conflictingDataSource.IsSequential ? "ToTarget() " : "ToTargetInstead() "
                : null;

            var existingDataSource = conflictingDataSource.GetDataSourceDescription();

            var reason = GetConflictReasonOrNull(conflictingDataSource);

            return $"{GetTargetMemberPath()} already has configured {toTarget}data source {existingDataSource}{reason}";
        }

        protected abstract string GetConflictReasonOrNull(ConfiguredDataSourceFactoryBase conflictingDataSource);

        public string GetDescription()
        {
            var sourceMemberPath = GetDataSourceDescription();
            var targetMemberPath = GetTargetMemberPath();

            return sourceMemberPath + " -> " + targetMemberPath;
        }

        protected string GetDataSourceDescription()
        {
            var description = DataSourceLambda.GetDescription(ConfigInfo);

            return DataSourceLambda.IsSourceMember ? description : "'" + description + "'";
        }

        protected string GetTargetMemberPath() => TargetMember.GetFriendlyTargetPath(ConfigInfo);

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