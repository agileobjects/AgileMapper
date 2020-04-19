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

        public virtual Expression GetConditionOrNull(IMemberMapperData mapperData)
            => ConfigInfo.GetConditionOrNull(mapperData);

        public bool CouldApplyTo(IQualifiedMemberContext context)
            => RuleSetMatches(context) && TypesMatch(context);

        public virtual bool AppliesTo(IQualifiedMemberContext context)
        {
            return RuleSetMatches(context) &&
                   TargetMembersMatch(context) &&
                   HasCompatibleCondition(context) &&
                   TypesMatch(context);
        }

        private bool RuleSetMatches(IRuleSetOwner ruleSetOwner) => ConfigInfo.IsFor(ruleSetOwner.RuleSet);

        private bool TargetMembersMatch(IQualifiedMemberContext context)
        {
            var otherTargetMember = context.TargetMember;

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

        protected virtual bool TypesMatch(IQualifiedMemberContext context)
            => SourceAndTargetTypesMatch(context);

        protected bool SourceAndTargetTypesMatch(IQualifiedMemberContext context)
        {
            if (TypesAreCompatible(context))
            {
                return true;
            }

            if (context.IsRoot)
            {
                return false;
            }

            context = context.Parent;

            while (true)
            {
                if (TypesAreCompatible(context))
                {
                    return true;
                }

                if (context.IsEntryPoint)
                {
                    return false;
                }

                context = context.Parent;
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

            if (ConfigInfo.HasSameSourceTypeAs(other))
            {
                if (ConfigInfo.HasSameTargetTypeAs(other))
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