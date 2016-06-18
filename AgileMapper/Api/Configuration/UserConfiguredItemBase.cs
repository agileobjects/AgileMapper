namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal abstract class UserConfiguredItemBase
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly QualifiedMember _targetMember;

        protected UserConfiguredItemBase(MappingConfigInfo configInfo)
            : this(configInfo, QualifiedMember.All)
        {
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : this(configInfo, GetTargetMember(configInfo, targetMemberLambda))
        {
        }

        protected static QualifiedMember GetTargetMember(
            MappingConfigInfo configInfo,
            LambdaExpression targetMemberLambda)
            => targetMemberLambda?.Body.ToTargetMember(configInfo.GlobalContext.MemberFinder);

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
        {
            _configInfo = configInfo;
            _targetMember = targetMember;
        }

        public string TargetMemberPath => _targetMember?.Path;

        public bool HasConfiguredCondition => _configInfo.HasCondition;

        public virtual bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasConfiguredCondition || otherConfiguredItem.HasConfiguredCondition)
            {
                return false;
            }

            if (SourceAndTargetTypesAreCompatible(otherConfiguredItem))
            {
                return _targetMember.Matches(otherConfiguredItem._targetMember);
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

        public Expression GetConditionOrNull(IMemberMappingContext context)
            => _configInfo.GetConditionOrNull(context);

        public virtual bool AppliesTo(IMappingData data)
        {
            return _configInfo.IsForRuleSet(data.RuleSetName) &&
                data.TargetMember.IsSameAs(_targetMember) &&
                ObjectHeirarchyHasMatchingSourceAndTargetTypes(data);
        }

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(IMappingData data)
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