namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }
    }
}