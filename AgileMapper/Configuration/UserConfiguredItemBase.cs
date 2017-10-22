namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal abstract class UserConfiguredItemBase : IComparable<UserConfiguredItemBase>
    {
        protected UserConfiguredItemBase(MappingConfigInfo configInfo)
            : this(configInfo, QualifiedMember.All)
        {
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : this(configInfo, GetTargetMemberOrThrow(targetMemberLambda))
        {
        }

        private static QualifiedMember GetTargetMemberOrThrow(LambdaExpression lambda)
        {
            var targetMember = lambda.Body.ToTargetMember(MapperContext.Default);

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

            if (HasOverlappingTypes(otherConfiguredItem) &&
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
                MemberPathHasMatchingSourceAndTargetTypes(mapperData);
        }

        private bool TargetMembersMatch(IBasicMapperData mapperData)
        {
            // The order of these checks is significant!
            if ((TargetMember == QualifiedMember.All) || (mapperData.TargetMember == QualifiedMember.All))
            {
                return true;
            }

            if (TargetMember == mapperData.TargetMember)
            {
                return true;
            }

            if ((TargetMember == QualifiedMember.None) || (mapperData.TargetMember == QualifiedMember.None))
            {
                return false;
            }

            return (mapperData.TargetMember.Type == TargetMember.Type) &&
                   (mapperData.TargetMember.Name == TargetMember.Name) &&
                   mapperData.TargetMember.LeafMember.DeclaringType.IsAssignableFrom(TargetMember.LeafMember.DeclaringType);
        }

        private bool MemberPathHasMatchingSourceAndTargetTypes(IBasicMapperData mapperData)
        {
            while (mapperData != null)
            {
                if (ConfigInfo.HasCompatibleTypes(mapperData))
                {
                    return true;
                }

                mapperData = mapperData.Parent;
            }

            return false;
        }

        int IComparable<UserConfiguredItemBase>.CompareTo(UserConfiguredItemBase other)
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

                return ConfigInfo.IsForTargetType(other.ConfigInfo) ? 1 : -1;
            }

            if (ConfigInfo.IsForSourceType(other.ConfigInfo))
            {
                // Derived source type
                return 1;
            }

            // Unrelated source and target types
            return GetConditionOrder(other) ?? -1;
        }

        private int? GetConditionOrder(UserConfiguredItemBase other)
        {
            if (!HasConfiguredCondition && other.HasConfiguredCondition)
            {
                return 1;
            }

            if (HasConfiguredCondition && !other.HasConfiguredCondition)
            {
                return -1;
            }

            return null;
        }
    }
}