namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;

    internal abstract class UserConfiguredItemBase
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
            var targetMember = lambda.Body.ToTargetMember(MapperContext.WithDefaultNamingSettings);

            if (targetMember != null)
            {
                return targetMember;
            }

            throw new MappingConfigurationException(
                $"Target member {lambda.Body.ToReadableString()} is not writeable.");
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            TargetMember = targetMember;
        }

        protected MappingConfigInfo ConfigInfo { get; }

        public QualifiedMember TargetMember { get; }

        public bool HasConfiguredCondition => ConfigInfo.HasCondition;

        public virtual bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasConfiguredCondition || otherConfiguredItem.HasConfiguredCondition)
            {
                return false;
            }

            if (ConfigInfo.HasCompatibleTypes(otherConfiguredItem.ConfigInfo))
            {
                return TargetMember.Matches(otherConfiguredItem.TargetMember);
            }

            return false;
        }

        protected bool SourceAndTargetTypesAreTheSame(UserConfiguredItemBase otherConfiguredItem)
        {
            return ConfigInfo.HasSameSourceTypeAs(otherConfiguredItem.ConfigInfo) &&
                   ConfigInfo.HasSameTargetTypeAs(otherConfiguredItem.ConfigInfo);
        }

        public virtual Expression GetConditionOrNull(IMemberMapperData mapperData)
            => GetConditionOrNull(mapperData, CallbackPosition.After);

        protected Expression GetConditionOrNull(IMemberMapperData mapperData, CallbackPosition position)
            => ConfigInfo.GetConditionOrNull(mapperData, position, TargetMember);

        public virtual bool AppliesTo(IBasicMapperData mapperData)
        {
            return ConfigInfo.IsFor(mapperData.RuleSet) &&
                TargetMembersMatch(mapperData) &&
                ObjectHeirarchyHasMatchingSourceAndTargetTypes(mapperData);
        }

        protected virtual bool TargetMembersMatch(IBasicMapperData mapperData)
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

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(IBasicMapperData mapperData)
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

        public static readonly IComparer<UserConfiguredItemBase> SpecificityComparer = new ConfiguredItemSpecificityComparer();

        private class ConfiguredItemSpecificityComparer : IComparer<UserConfiguredItemBase>
        {
            public int Compare(UserConfiguredItemBase x, UserConfiguredItemBase y)
            {
                if (!x.HasConfiguredCondition && y.HasConfiguredCondition)
                {
                    return 1;
                }

                if (x.ConfigInfo.HasSameSourceTypeAs(y.ConfigInfo))
                {
                    return 0;
                }

                if (x.ConfigInfo.IsForSourceType(y.ConfigInfo))
                {
                    return 1;
                }

                return -1;
            }
        }
    }
}