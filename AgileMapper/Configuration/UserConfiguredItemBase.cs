namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;
    using ReadableExpressions;

    internal abstract class UserConfiguredItemBase
    {
        private readonly MappingConfigInfo _configInfo;

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
            _configInfo = configInfo;
            TargetMember = targetMember;
        }

        public QualifiedMember TargetMember { get; }

        public bool HasConfiguredCondition => _configInfo.HasCondition;

        public virtual bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasConfiguredCondition || otherConfiguredItem.HasConfiguredCondition)
            {
                return false;
            }

            if (_configInfo.HasCompatibleTypes(otherConfiguredItem._configInfo))
            {
                return TargetMember.Matches(otherConfiguredItem.TargetMember);
            }

            return false;
        }

        protected bool SourceAndTargetTypesAreTheSame(UserConfiguredItemBase otherConfiguredItem)
        {
            return _configInfo.HasSameSourceTypeAs(otherConfiguredItem._configInfo) &&
                   _configInfo.HasSameTargetTypeAs(otherConfiguredItem._configInfo);
        }

        public virtual Expression GetConditionOrNull(IMemberMapperData mapperData)
            => _configInfo.GetConditionOrNull(mapperData);

        public virtual bool AppliesTo(IBasicMapperData mapperData)
        {
            return _configInfo.IsFor(mapperData.RuleSet) &&
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
                if (_configInfo.HasCompatibleTypes(mapperData))
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

                if (x._configInfo.HasSameSourceTypeAs(y._configInfo))
                {
                    return 0;
                }

                if (x._configInfo.IsForSourceType(y._configInfo))
                {
                    return 1;
                }

                return -1;
            }
        }
    }
}