namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
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
            var targetMember = lambda.Body.ToTargetMember(
                GlobalContext.Instance.MemberFinder,
                MapperContext.WithDefaultNamingSettings);

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

            if (SourceAndTargetTypesAreCompatible(otherConfiguredItem))
            {
                return TargetMember.Matches(otherConfiguredItem.TargetMember);
            }

            return false;
        }

        private bool SourceAndTargetTypesAreCompatible(UserConfiguredItemBase otherConfiguredItem)
        {
            return _configInfo.IsForSourceType(otherConfiguredItem._configInfo) &&
                   _configInfo.IsForTargetType(otherConfiguredItem._configInfo);
        }

        protected bool SourceAndTargetTypesAreTheSame(UserConfiguredItemBase otherConfiguredItem)
        {
            return _configInfo.HasSameSourceTypeAs(otherConfiguredItem._configInfo) &&
                   _configInfo.HasSameTargetTypeAs(otherConfiguredItem._configInfo);
        }

        public virtual Expression GetConditionOrNull(MemberMapperData mapperData)
            => _configInfo.GetConditionOrNull(mapperData);

        public virtual bool AppliesTo(BasicMapperData data)
        {
            return _configInfo.IsFor(data.RuleSet) &&
                data.TargetMember.IsSameAs(TargetMember) &&
                ObjectHeirarchyHasMatchingSourceAndTargetTypes(data);
        }

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(BasicMapperData data)
        {
            while (data != null)
            {
                if (_configInfo.IsForSourceType(data.SourceType) &&
                    _configInfo.IsForTargetType(data.TargetMember.Type ?? data.TargetType))
                {
                    return true;
                }

                data = data.Parent;
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