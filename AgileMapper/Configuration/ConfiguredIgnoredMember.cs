namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        private readonly TargetMemberSelector _memberSelector;

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, Action<TargetMemberSelector> memberFilter)
            : base(configInfo, QualifiedMember.All)
        {
            _memberSelector = new TargetMemberSelector();

            memberFilter.Invoke(_memberSelector);
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            return (_memberSelector == null) || _memberSelector.Matches(mapperData);
        }
    }
}