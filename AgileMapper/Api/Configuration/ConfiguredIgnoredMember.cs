namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        public ConfiguredIgnoredMember(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            LambdaExpression targetMemberLambda)
            : base(configInfo, mappingTargetType, targetMemberLambda)
        {
        }

        public bool ConflictsWith(ConfiguredIgnoredMember otherIgnoredMember)
        {
            if (HasConfiguredCondition || otherIgnoredMember.HasConfiguredCondition)
            {
                return false;
            }

            return TargetMember.Matches(otherIgnoredMember.TargetMember);
        }
    }
}