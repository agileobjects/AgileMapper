namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal abstract class UserConfiguredItemBase
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly Type _mappingTargetType;
        private readonly QualifiedMember _targetMember;

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, Type mappingTargetType)
            : this(configInfo, mappingTargetType, QualifiedMember.All)
        {
        }

        protected UserConfiguredItemBase(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            LambdaExpression targetMemberLambda)
            : this(
                  configInfo,
                  mappingTargetType,
                  targetMemberLambda.Body.ToTargetMember(configInfo.GlobalContext.MemberFinder))
        {
            TargetMemberPath = targetMemberLambda.Body.ToReadableString();
        }

        protected UserConfiguredItemBase(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            QualifiedMember targetMember)
        {
            _configInfo = configInfo;
            _mappingTargetType = mappingTargetType;
            _targetMember = targetMember;
        }

        public string TargetMemberPath { get; }

        public bool HasConfiguredCondition => _configInfo.HasCondition;

        public bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (HasConfiguredCondition || otherConfiguredItem.HasConfiguredCondition)
            {
                return false;
            }

            return _targetMember.Matches(otherConfiguredItem._targetMember);
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
                if (_mappingTargetType.IsAssignableFrom(data.TargetType) &&
                    _configInfo.IsForSourceType(data.SourceType))
                {
                    return true;
                }

                data = data.Parent;
            }

            return false;
        }
    }
}