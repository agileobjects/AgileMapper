namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredIgnoredMember : UserConfiguredItemBase
    {
        private readonly Func<TargetMemberSelector, bool> _memberFilter;

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        public ConfiguredIgnoredMember(MappingConfigInfo configInfo, Func<TargetMemberSelector, bool> memberFilter)
            : base(configInfo, QualifiedMember.All)
        {
            _memberFilter = memberFilter;
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (!base.AppliesTo(mapperData))
            {
                return false;
            }

            return (_memberFilter == null) ||
                    _memberFilter.Invoke(new TargetMemberSelector(mapperData.TargetMember));
        }

        protected override bool MembersConflict(QualifiedMember otherMember)
        {
            if (_memberFilter == null)
            {
                return base.MembersConflict(otherMember);
            }

            return _memberFilter.Invoke(new TargetMemberSelector(otherMember));
        }
    }
}