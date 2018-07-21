namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            var targetMember = lambda.ToTargetMember(configInfo.MapperContext);

            if (targetMember == null)
            {
                throw new MappingConfigurationException(
                    $"Target member {lambda.Body.ToReadableString()} is not writeable");
            }

            if (targetMember.IsUnmappable(out var reason))
            {
                throw new MappingConfigurationException(
                    $"Target member {lambda.Body.ToReadableString()} is not mappable ({reason})");
            }

            return targetMember;
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            TargetMember = targetMember;
        }

        public MappingConfigInfo ConfigInfo { get; }

        public string TargetTypeName => ConfigInfo.TargetType.GetFriendlyName();

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

        public virtual bool AppliesTo(IBasicMapperData mapperData)
        {
            return ConfigInfo.IsFor(mapperData.RuleSet) &&
                   TargetMembersMatch(mapperData) &&
                   HasCompatibleCondition(mapperData) &&
                   MemberPathMatches(mapperData);
        }

        private bool TargetMembersMatch(IBasicMapperData mapperData)
        {
            // The order of these checks is significant!
            if ((TargetMember == QualifiedMember.All) || (mapperData.TargetMember == QualifiedMember.All))
            {
                return true;
            }

            if (TargetMembersAreCompatible(mapperData))
            {
                return true;
            }

            if ((TargetMember == QualifiedMember.None) || (mapperData.TargetMember == QualifiedMember.None))
            {
                return false;
            }

            return (mapperData.TargetMember.Type == TargetMember.Type) &&
                   (mapperData.TargetMember.Name == TargetMember.Name) &&
                    TargetMember.LeafMember.DeclaringType.IsAssignableTo(mapperData.TargetMember.LeafMember.DeclaringType);
        }

        protected virtual bool TargetMembersAreCompatible(IBasicMapperData mapperData)
            => TargetMember == mapperData.TargetMember;

        private bool HasCompatibleCondition(IBasicMapperData mapperData)
            => !HasConfiguredCondition || ConfigInfo.ConditionSupports(mapperData.RuleSet);

        protected virtual bool MemberPathMatches(IBasicMapperData mapperData)
            => MemberPathHasMatchingSourceAndTargetTypes(mapperData);

        protected bool MemberPathHasMatchingSourceAndTargetTypes(IBasicMapperData mapperData)
        {
            while (mapperData != null)
            {
                if (mapperData.HasCompatibleTypes(ConfigInfo))
                {
                    return true;
                }

                mapperData = mapperData.Parent;
            }

            return false;
        }

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
                ConfigInfo.TargetType.Name,
                other.ConfigInfo.TargetType.Name,
                StringComparison.Ordinal);
        }
    }
}