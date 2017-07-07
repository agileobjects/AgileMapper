namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        private readonly Func<TargetMemberSelector, TargetMemberSelector> _memberSelectorConfigurator;

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, Action<TargetMemberSelector> memberFilter)
            : base(configInfo, QualifiedMember.All)
        {
            _memberSelectorConfigurator = selector =>
            {
                memberFilter.Invoke(selector);

                return selector;
            };
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            if (_memberSelectorConfigurator == null)
            {
                return true;
            }

            return _memberSelectorConfigurator
                .Invoke(new TargetMemberSelector())
                .Matches(mapperData);
        }
    }
}