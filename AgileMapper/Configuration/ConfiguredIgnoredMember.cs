namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase, IPotentialClone
    {
        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        private ConfiguredIgnoredMember(MappingConfigInfo configInfo, QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
        }

        public bool IsClone { get; private set; }

        public IPotentialClone Clone()
        {
            return new ConfiguredIgnoredMember(ConfigInfo, TargetMember)
            {
                IsClone = true
            };
        }
    }
}