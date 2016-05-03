namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal abstract class UserConfiguredItemBase
    {
        private readonly Type _targetType;
        private readonly QualifiedMember _targetMember;
        private Func<IConfigurationContext, Expression> _conditionFactory;

        protected UserConfiguredItemBase(
            MappingConfigInfo configInfo,
            Type targetType,
            QualifiedMember targetMember)
        {
            ConfigInfo = configInfo;
            _targetType = targetType;
            _targetMember = targetMember;
        }

        protected MappingConfigInfo ConfigInfo { get; }

        public void AddCondition(Func<IConfigurationContext, Expression> conditionFactory)
        {
            _conditionFactory = conditionFactory;
        }

        public Expression GetCondition(IConfigurationContext context)
        {
            return _conditionFactory?.Invoke(context);
        }

        public bool AppliesTo(IConfigurationContext context)
        {
            if (!ConfigInfo.IsForRuleSet(context.RuleSetName))
            {
                return false;
            }

            if (!context.TargetMember.Equals(_targetMember))
            {
                return false;
            }

            return ConfigInfo.IsForAllSources || ObjectHeirarchyHasMatchingSourceAndTargetTypes(context);
        }

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(IConfigurationContext context)
        {
            while (context != null)
            {
                if (_targetType.IsAssignableFrom(context.ExistingObjectType) &&
                    ConfigInfo.IsForSourceType(context.SourceObjectType))
                {
                    return true;
                }

                context = context.Parent;
            }

            return false;
        }
    }
}