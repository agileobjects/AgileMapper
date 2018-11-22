namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using System;
#endif
    using Configuration;
    using Members;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

        public ConfiguredDataSourceFactory CreateReverseIfAppropriate(bool isAutoReversal)
        {
            if ((_valueCouldBeSourceMember == false) || ConfigInfo.HasCondition)
            {
                return null;
            }

            if (!_dataSourceLambda.IsSourceMember(out var sourceMemberLambda))
            {
                return null;
            }

            var targetMember = sourceMemberLambda.ToTargetMemberOrNull(ConfigInfo.MapperContext, out _);

            if (targetMember == null)
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
                WasAutoCreated = isAutoReversal,
                _isReversal = true
            };
        }

        public MappingConfigInfo GetReverseConfigInfo()
        {
            return _reverseConfigInfo ?? (_reverseConfigInfo = ConfigInfo
                .Copy()
                .ForSourceType(ConfigInfo.TargetType)
                .ForTargetType(ConfigInfo.SourceType)
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

        private string GetTargetMemberPath()
            => TargetMember.GetFriendlyMemberPath(ConfigInfo.TargetType.GetFriendlyName(), Member.RootTargetMemberName);

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