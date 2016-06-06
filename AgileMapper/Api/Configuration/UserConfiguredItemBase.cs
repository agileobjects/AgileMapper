namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal abstract class UserConfiguredItemBase
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly QualifiedMember _targetMember;

        protected UserConfiguredItemBase(MappingConfigInfo configInfo)
            : this(configInfo, QualifiedMember.All)
        {
        }

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : this(
                  configInfo,
                  targetMemberLambda.Body.ToTargetMember(configInfo.GlobalContext.MemberFinder))
        {
            TargetMemberPath = targetMemberLambda.Body.ToReadableString();
        }

        private UserConfiguredItemBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
        {
            _configInfo = configInfo;
            _targetMember = targetMember;
        }

        public string TargetMemberPath { get; }

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

        public Expression GetCondition(IMemberMappingContext context)
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
                    _configInfo.IsForTargetType(data.TargetType))
                {
                    return true;
                }

                data = data.Parent;
            }

            return false;
        }
    }
}