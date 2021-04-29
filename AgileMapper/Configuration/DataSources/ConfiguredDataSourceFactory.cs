namespace AgileObjects.AgileMapper.Configuration.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Lambdas;
    using Members;

    internal class ConfiguredDataSourceFactory :
        ConfiguredDataSourceFactoryBase,
        IReversibleConfiguredDataSourceFactory
    {
        private bool _isReversal;
        private MappingConfigInfo _reverseConfigInfo;

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember targetMember)
            : base(configInfo, dataSourceLambda, targetMember)
        {
        }

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            LambdaExpression targetMemberLambda,
            bool valueCouldBeSourceMember)
            : base(configInfo, dataSourceLambda, targetMemberLambda, valueCouldBeSourceMember)
        {
        }

        public bool CannotBeReversed(out string reason) => CannotBeReversed(out _, out reason);

        private bool CannotBeReversed(out QualifiedMember targetMember, out string reason)
        {
            if (ValueCouldBeSourceMember == false)
            {
                targetMember = null;
                reason = $"configured value '{DataSourceLambda.GetDescription(ConfigInfo)}' is not a source member";
                return true;
            }

            if (ConfigInfo.HasCondition)
            {
                targetMember = null;
                reason = $"configuration has condition '{ConfigInfo.GetConditionDescription()}'";
                return true;
            }

            if (!TargetMember.IsReadable)
            {
                targetMember = null;
                reason = $"target member '{GetTargetDescription()}' is not readable, so cannot be used as a source member";
                return true;
            }

            if (!DataSourceLambda.TryGetSourceMember(out var sourceMemberLambda))
            {
                targetMember = null;
                reason = $"configured value '{DataSourceLambda.GetDescription(ConfigInfo)}' is not a source member";
                return true;
            }

            targetMember = sourceMemberLambda.ToTargetMemberOrNull(
                SourceType,
                ConfigInfo.MapperContext,
                out reason);

            if (targetMember != null)
            {
                return false;
            }

            var sourceMember = sourceMemberLambda.ToSourceMember(ConfigInfo.MapperContext);
            var sourceMemberPath = sourceMember.GetFriendlySourcePath(ConfigInfo);

            reason = $"source member '{sourceMemberPath}' is not a useable target member. {reason}";
            return true;

        }

        public ConfiguredDataSourceFactoryBase CreateReverseIfAppropriate(bool isAutoReversal)
        {
            if (CannotBeReversed(out var targetMember, out _))
            {
                return null;
            }

            var reverseConfigInfo = GetReverseConfigInfo();

            var sourceParameter = Parameters.Create(reverseConfigInfo.SourceType, "source");
            var sourceMemberAccess = TargetMember.GetQualifiedAccess(sourceParameter);

            var sourceMemberAccessLambda = Expression.Lambda(
                Expression.GetFuncType(sourceParameter.Type, sourceMemberAccess.Type),
                sourceMemberAccess,
                sourceParameter);

            var sourceMemberLambdaInfo = ConfiguredLambdaInfo.For(sourceMemberAccessLambda, reverseConfigInfo);

            return new ConfiguredDataSourceFactory(reverseConfigInfo, sourceMemberLambdaInfo, targetMember)
            {
                _isReversal = true,
                WasAutoCreated = isAutoReversal
            };
        }

        public MappingConfigInfo GetReverseConfigInfo()
        {
            return _reverseConfigInfo ??= ConfigInfo
                .Copy()
                .ForSourceType(TargetType)
                .ForTargetType(SourceType)
                .ForSourceValueType(TargetMember.Type);
        }

        #region ConflictsWith Helpers

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
            => TargetMember.LeafMember.Equals(otherConfiguredItem.TargetMember.LeafMember);

        protected override bool HasSameCriteriaAs(ConfiguredDataSourceFactoryBase otherDataSource)
            => DataSourceLambda.IsSameAs(otherDataSource?.DataSourceLambda);

        #endregion

        protected override string GetToTargetDescription(ConfiguredDataSourceFactoryBase conflictingDataSource)
        {
            return TargetMember.IsRoot
                ? conflictingDataSource.IsSequential ? "ToTarget() " : "ToTargetInstead() "
                : null;
        }

        protected override string GetConflictReasonOrNull(ConfiguredDataSourceFactoryBase conflictingDataSource)
        {
            return conflictingDataSource is ConfiguredDataSourceFactory dsf && dsf._isReversal
                ? " from an automatically-configured reverse data source" : null;
        }

        public override string GetDescription()
            => GetDataSourceDescription() + " -> " + GetTargetDescription();

        protected override string GetDataSourceDescription() => GetDataSourceValueDescription();

        protected override string GetTargetDescription() => TargetMember.GetFriendlyTargetPath(ConfigInfo);

        protected override bool TargetMembersAreCompatibleForToTarget(QualifiedMember otherTargetMember)
            => TargetMember.HasCompatibleType(otherTargetMember.Type);

        #region IPotentialAutoCreatedItem Members

        public override IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredDataSourceFactory(ConfigInfo, DataSourceLambda, TargetMember)
            {
                ValueCouldBeSourceMember = ValueCouldBeSourceMember,
                WasAutoCreated = true
            };
        }

        public override bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedDataSourceFactory)
            => ConflictsWith((ConfiguredDataSourceFactory)autoCreatedDataSourceFactory);

        #endregion
    }
}