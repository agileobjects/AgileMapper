namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal abstract class UserConfiguredItemBase
    {
        private readonly Type _mappingTargetType;
        private readonly IQualifiedMember _targetMember;
        private Func<IMemberMappingContext, Expression> _conditionFactory;

        protected UserConfiguredItemBase(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            IQualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            _mappingTargetType = mappingTargetType;
            _targetMember = targetMember;
        }

        protected MappingConfigInfo ConfigInfo { get; }

        public void AddConditionFactory(Func<IMemberMappingContext, Expression> conditionFactory)
        {
            _conditionFactory = conditionFactory;
        }

        public Expression GetCondition(IMemberMappingContext context)
            => _conditionFactory?.Invoke(context);

        public virtual bool AppliesTo(IMemberMappingContext context)
        {
            return ConfigInfo.IsForRuleSet(context.RuleSetName) &&
                context.TargetMember.IsSameAs(_targetMember) &&
                ObjectHeirarchyHasMatchingSourceAndTargetTypes(context);
        }

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(IMemberMappingContext context)
        {
            while (context != null)
            {
                if (_mappingTargetType.IsAssignableFrom(context.ExistingObject.Type) &&
                    ConfigInfo.IsForSourceType(context.SourceObject.Type))
                {
                    return true;
                }

                context = context.Parent;
            }

            return false;
        }
    }
}