namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using System;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Lambdas;
    using Members;

    internal class ConfiguredDataSourceFactory :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredDataSourceFactory>
#endif
    {
        private readonly ConfiguredLambdaInfo _dataSourceLambda;
        private bool _valueCouldBeSourceMember;
        private MappingConfigInfo _reverseConfigInfo;
        private bool _isReversal;

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
            _dataSourceLambda = dataSourceLambda;
        }

        public ConfiguredDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            LambdaExpression targetMemberLambda,
            bool valueCouldBeSourceMember)
            : base(configInfo, targetMemberLambda)
        {
            _valueCouldBeSourceMember = valueCouldBeSourceMember;
            _dataSourceLambda = dataSourceLambda;
        }

        public bool IsForToTargetDataSource => TargetMember.IsRoot;

        public bool IsSequential => ConfigInfo.IsSequentialConfiguration;

        public bool CannotBeReversed(out string reason) => CannotBeReversed(out _, out reason);

        private bool CannotBeReversed(out QualifiedMember targetMember, out string reason)
        {
            if (_valueCouldBeSourceMember == false)
            {
                targetMember = null;
                reason = $"configured value '{_dataSourceLambda.GetDescription(ConfigInfo)}' is not a source member";
                return true;
            }

            if (ConfigInfo.HasCondition)
            {
                targetMember = null;
                reason = $"configuration has condition '{ConfigInfo.GetConditionDescription(ConfigInfo)}'";
                return true;
            }

            if (!TargetMember.IsReadable)
            {
                targetMember = null;
                reason = $"target member '{GetTargetMemberPath()}' is not readable, so cannot be used as a source member";
                return true;
            }

            if (!_dataSourceLambda.TryGetSourceMember(out var sourceMemberLambda))
            {
                targetMember = null;
                reason = $"configured value '{_dataSourceLambda.GetDescription(ConfigInfo)}' is not a source member";
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

        public ConfiguredDataSourceFactory CreateReverseIfAppropriate(bool isAutoReversal)
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

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherDataSource = otherConfiguredItem as ConfiguredDataSourceFactory;
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

        private bool HasSameDataSourceAs(ConfiguredDataSourceFactory otherDataSource)
            => _dataSourceLambda.IsSameAs(otherDataSource?._dataSourceLambda);

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
            => TargetMember.LeafMember.Equals(otherConfiguredItem.TargetMember.LeafMember);

        #endregion

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            var toTarget = TargetMember.IsRoot
                ? conflictingDataSource.IsSequential ? "ToTarget() " : "ToTargetInstead() "
                : null;

            var existingDataSource = conflictingDataSource.GetDataSourceDescription();

            var reason = conflictingDataSource._isReversal
                ? " from an automatically-configured reverse data source" : null;

            return $"{GetTargetMemberPath()} already has configured {toTarget}data source {existingDataSource}{reason}";
        }

        public string GetDescription()
        {
            var sourceMemberPath = GetDataSourceDescription();
            var targetMemberPath = GetTargetMemberPath();

            return sourceMemberPath + " -> " + targetMemberPath;
        }

        private string GetDataSourceDescription()
        {
            var description = _dataSourceLambda.GetDescription(ConfigInfo);

            return _dataSourceLambda.IsSourceMember ? description : "'" + description + "'";
        }

        private string GetTargetMemberPath() => TargetMember.GetFriendlyTargetPath(ConfigInfo);

        public override bool AppliesTo(IQualifiedMemberContext context)
            => base.AppliesTo(context) && _dataSourceLambda.Supports(context.RuleSet);

        protected override bool TargetMembersAreCompatible(IQualifiedMember otherTargetMember)
        {
            if (base.TargetMembersAreCompatible(otherTargetMember))
            {
                return true;
            }

            return TargetMember.IsRoot && TargetMember.HasCompatibleType(otherTargetMember.Type);
        }

        public IConfiguredDataSource Create(IMemberMapperData mapperData)
        {
            var configuredCondition = GetConditionOrNull(mapperData);
            var value = _dataSourceLambda.GetBody(mapperData);

            return new ConfiguredDataSource(
                configuredCondition,
                value,
                ConfigInfo.IsSequentialConfiguration,
                mapperData);
        }

        public QualifiedMember ToSourceMemberOrNull()
        {
            if (_valueCouldBeSourceMember &&
                _dataSourceLambda.TryGetSourceMember(out var sourceMemberLambda))
            {
                return sourceMemberLambda.ToSourceMemberOrNull(ConfigInfo.MapperContext);
            }

            return null;
        }

        protected override int? GetSameTypesOrder(UserConfiguredItemBase other)
            => ((ConfiguredDataSourceFactory)other).IsSequential ? -1 : base.GetSameTypesOrder(other);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; private set; }

        public IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredDataSourceFactory(ConfigInfo, _dataSourceLambda, TargetMember)
            {
                _valueCouldBeSourceMember = _valueCouldBeSourceMember,
                WasAutoCreated = true
            };
        }

        public bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedDataSourceFactory)
            => ConflictsWith((ConfiguredDataSourceFactory)autoCreatedDataSourceFactory);

        #endregion

#if NET35
        int IComparable<ConfiguredDataSourceFactory>.CompareTo(ConfiguredDataSourceFactory other)
            => DoComparisonTo(other);
#endif
    }
}