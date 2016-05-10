namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal abstract class UserConfiguredItemBase
    {
        private readonly Type _mappingTargetType;
        private readonly QualifiedMember _targetMember;
        private Func<ParameterExpression, Expression> _conditionFactory;

        protected UserConfiguredItemBase(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            QualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            _mappingTargetType = mappingTargetType;
            _targetMember = targetMember;
        }

        protected MappingConfigInfo ConfigInfo { get; }

        public void AddConditionFactory(Func<ParameterExpression, Expression> conditionFactory)
        {
            _conditionFactory = conditionFactory;
        }

        public Expression GetCondition(ParameterExpression contextParameter)
        {
            return _conditionFactory?.Invoke(contextParameter);
        }

        public virtual bool AppliesTo(IMemberMappingContext context)
        {
            if (!ConfigInfo.IsForRuleSet(context.RuleSetName))
            {
                return false;
            }

            if (!context.TargetMember.Equals(_targetMember))
            {
                return false;
            }

            return ObjectHeirarchyHasMatchingSourceAndTargetTypes(context);
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