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

        protected UserConfiguredItemBase(MappingConfigInfo configInfo, Type mappingTargetType)
            : this(configInfo, mappingTargetType, QualifiedMember.All)
        {
        }

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

        public virtual bool AppliesTo(IMappingData data)
        {
            return ConfigInfo.IsForRuleSet(data.RuleSetName) &&
                data.TargetMember.IsSameAs(_targetMember) &&
                ObjectHeirarchyHasMatchingSourceAndTargetTypes(data);
        }

        private bool ObjectHeirarchyHasMatchingSourceAndTargetTypes(IMappingData data)
        {
            while (data != null)
            {
                if (_mappingTargetType.IsAssignableFrom(data.TargetType) &&
                    ConfigInfo.IsForSourceType(data.SourceType))
                {
                    return true;
                }

                data = data.Parent;
            }

            return false;
        }
    }
}