namespace AgileObjects.AgileMapper.DataSources.Factories
{
#if NET35
    using System;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
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

            if (!_dataSourceLambda.IsSourceMember(out var sourceMemberLambda))
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

            var sourceMemberLambdaInfo = ConfiguredLambdaInfo.For(sourceMemberAccessLambda);

            return new ConfiguredDataSourceFactory(reverseConfigInfo, sourceMemberLambdaInfo, targetMember)
            {
                _isReversal = true,
                WasAutoCreated = isAutoReversal
            };
        }

        public MappingConfigInfo GetReverseConfigInfo()
        {
            return _reverseConfigInfo ?? (_reverseConfigInfo = ConfigInfo
                .Copy()
                .ForSourceType(TargetType)
                .ForTargetType(SourceType)
                .ForSourceValueType(TargetMember.Type));
        }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherDataSource = otherConfiguredItem as ConfiguredDataSourceFactory;
            var isOtherDataSource = otherDataSource != null;
            var dataSourceLambdasAreTheSame = HasSameDataSourceLambdaAs(otherDataSource);

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

            if (SourceAndTargetTypesAreTheSame(otherDataSource))
            {
                return true;
            }

            return dataSourceLambdasAreTheSame;
        }

        #region ConflictsWith Helpers

        private bool HasSameDataSourceLambdaAs(ConfiguredDataSourceFactory otherDataSource)
            => _dataSourceLambda.IsSameAs(otherDataSource?._dataSourceLambda);

        protected override bool MembersConflict(UserConfiguredItemBase otherConfiguredItem)
            => TargetMember.LeafMember.Equals(otherConfiguredItem.TargetMember.LeafMember);

        #endregion

        public string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource)
        {
            var existingDataSource = conflictingDataSource.GetDataSourceDescription();

            var reason = conflictingDataSource._isReversal
                ? " from an automatically-configured reverse data source" : null;


            return $"{GetTargetMemberPath()} already has configured data source '{existingDataSource}'{reason}";
        }

        public string GetDescription()
        {
            var sourceMemberPath = GetDataSourceDescription();
            var targetMemberPath = GetTargetMemberPath();

            return sourceMemberPath + " -> " + targetMemberPath;
        }

        private string GetDataSourceDescription() => _dataSourceLambda.GetDescription(ConfigInfo);

        private string GetTargetMemberPath() => TargetMember.GetFriendlyTargetPath(ConfigInfo);

        public override bool AppliesTo(IBasicMapperData mapperData)
            => base.AppliesTo(mapperData) && _dataSourceLambda.Supports(mapperData.RuleSet);

        protected override bool TargetMembersAreCompatible(IBasicMapperData mapperData)
        {
            if (base.TargetMembersAreCompatible(mapperData))
            {
                return true;
            }

            return TargetMember.IsRoot && TargetMember.HasCompatibleType(mapperData.TargetMember.Type);
        }

        public IConfiguredDataSource Create(IMemberMapperData mapperData)
        {
            var configuredCondition = GetConditionOrNull(mapperData);
            var value = _dataSourceLambda.GetBody(mapperData);

            return new ConfiguredDataSource(configuredCondition, value, mapperData);
        }

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