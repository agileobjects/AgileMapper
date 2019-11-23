namespace AgileObjects.AgileMapper.Configuration
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal abstract class UserConfiguredItemBase : IComparable<UserConfiguredItemBase>
    {
        protected UserConfiguredItemBase(MappingConfigInfo configInfo)
            : this(configInfo, QualifiedMember.All)
        {
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : this(configInfo, GetTargetMemberOrThrow(targetMemberLambda, configInfo))
        {
        }

        private static QualifiedMember GetTargetMemberOrThrow(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            var targetMember = lambda.ToTargetMemberOrNull(
                configInfo.TargetType,
                configInfo.MapperContext,
                out var failureReason);

            return targetMember ?? throw new MappingConfigurationException(failureReason);
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            TargetMember = targetMember;
        }

        public MappingConfigInfo ConfigInfo { get; }

        public Type SourceType => ConfigInfo.SourceType;

        public string SourceTypeName => SourceType.GetFriendlyName();

        public Type TargetType => ConfigInfo.TargetType;

        public string TargetTypeName => TargetType.GetFriendlyName();

        public QualifiedMember TargetMember { get; }

        public bool HasConfiguredCondition => ConfigInfo.HasCondition;

        public virtual bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasReverseConflict(otherConfiguredItem))
            {
                return true;
            }

            if (HasOverlappingRuleSets(otherConfiguredItem) &&
                HasOverlappingTypes(otherConfiguredItem) &&
                MembersConflict(otherConfiguredItem))
            {
                return !(HasConfiguredCondition || otherConfiguredItem.HasConfiguredCondition);
            }

            return false;
        }

        protected virtual bool HasReverseConflict(UserConfiguredItemBase otherItem)
        {
            return otherItem is IReverseConflictable conflictable && conflictable.ConflictsWith(this);
        }

        private bool HasOverlappingRuleSets(UserConfiguredItemBase otherItem)
        {
            return ConfigInfo.IsFor(otherItem.ConfigInfo.RuleSet) ||
                   otherItem.ConfigInfo.IsFor(ConfigInfo.RuleSet);
        }

        protected virtual bool HasOverlappingTypes(UserConfiguredItemBase otherItem)
            => ConfigInfo.HasCompatibleTypes(otherItem.ConfigInfo);

        protected virtual bool MembersConflict(UserConfiguredItemBase otherItem)
            => TargetMember.Matches(otherItem.TargetMember);

        protected bool SourceAndTargetTypesAreTheSame(UserConfiguredItemBase otherItem)
        {
            return ConfigInfo.HasSameSourceTypeAs(otherItem.ConfigInfo) &&
                   ConfigInfo.HasSameTargetTypeAs(otherItem.ConfigInfo);
        }

        public Expression GetConditionOrNull(IMemberMapperData mapperData)
            => GetConditionOrNull(mapperData, CallbackPosition.After);

        protected virtual Expression GetConditionOrNull(IMemberMapperData mapperData, CallbackPosition position)
            => ConfigInfo.GetConditionOrNull(mapperData, position, TargetMember);

        public bool CouldApplyTo(IBasicMapperData mapperData)
            => RuleSetMatches(mapperData) && TypesMatch(mapperData);

        public virtual bool AppliesTo(IBasicMapperData mapperData)
        {
            return RuleSetMatches(mapperData) &&
                   TargetMembersMatch(mapperData) &&
                   HasCompatibleCondition(mapperData) &&
                   TypesMatch(mapperData);
        }

        private bool RuleSetMatches(IRuleSetOwner ruleSetOwner) => ConfigInfo.IsFor(ruleSetOwner.RuleSet);

        private bool TargetMembersMatch(IBasicMapperData mapperData)
        {
            var otherTargetMember = mapperData.TargetMember;

            // The order of these checks is significant!
            if ((TargetMember == QualifiedMember.All) || (otherTargetMember == QualifiedMember.All))
            {
                return true;
            }

            if (TargetMembersAreCompatible(otherTargetMember))
            {
                return true;
            }

            if ((TargetMember == QualifiedMember.None) || (otherTargetMember == QualifiedMember.None))
            {
                return false;
            }

            return (otherTargetMember.Type == TargetMember.Type) &&
                   (otherTargetMember.Name == TargetMember.Name) &&
                    otherTargetMember.LeafMember.DeclaringType.IsAssignableTo(TargetMember.LeafMember.DeclaringType);
        }

        protected virtual bool TargetMembersAreCompatible(IQualifiedMember otherTargetMember)
            => TargetMember == otherTargetMember;

        private bool HasCompatibleCondition(IRuleSetOwner ruleSetOwner)
            => !HasConfiguredCondition || ConfigInfo.ConditionSupports(ruleSetOwner.RuleSet);

        protected virtual bool TypesMatch(IBasicMapperData mapperData)
            => SourceAndTargetTypesMatch(mapperData);

        protected bool SourceAndTargetTypesMatch(IBasicMapperData mapperData)
        {
            if (TypesAreCompatible(mapperData))
            {
                return true;
            }

            if (mapperData.IsRoot)
            {
                return false;
            }

            var parentMapperData = mapperData.Parent;

            while (true)
            {
                if (TypesAreCompatible(parentMapperData))
                {
                    return true;
                }

                if (parentMapperData.IsEntryPoint)
                {
                    return false;
                }

                parentMapperData = parentMapperData.Parent;
            }
        }

        protected bool TypesAreCompatible(ITypePair typePair) => typePair.HasCompatibleTypes(ConfigInfo);

        int IComparable<UserConfiguredItemBase>.CompareTo(UserConfiguredItemBase other)
            => DoComparisonTo(other);

        protected int DoComparisonTo(UserConfiguredItemBase other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ConfigInfo.HasSameSourceTypeAs(other.ConfigInfo))
            {
                if (ConfigInfo.HasSameTargetTypeAs(other.ConfigInfo))
                {
                    return GetConditionOrder(other) ?? 0;
                }

                if (ConfigInfo.IsForTargetType(other.ConfigInfo))
                {
                    // Derived target type
                    return 1;
                }

                return OrderAlphabetically(other);
            }

            if (ConfigInfo.IsForSourceType(other.ConfigInfo))
            {
                // Derived source type
                return 1;
            }

            // Unrelated source and target types
            return GetConditionOrder(other) ?? OrderAlphabetically(other);
        }

        private int? GetConditionOrder(UserConfiguredItemBase other)
        {
            if (HasConfiguredCondition == other.HasConfiguredCondition)
            {
                return 0;
            }

            if (HasConfiguredCondition)
            {
                return -1;
            }

            if (other.HasConfiguredCondition)
            {
                return 1;
            }

            return null;
        }

        private int OrderAlphabetically(UserConfiguredItemBase other)
        {
            return string.Compare(
                TargetTypeName,
                other.TargetTypeName,
                StringComparison.Ordinal);
        }
    }
}