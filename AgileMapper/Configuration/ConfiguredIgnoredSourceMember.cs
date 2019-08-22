namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
#if NET35
    using Extensions.Internal;
#endif
    using Members;

    internal class ConfiguredIgnoredSourceMember : UserConfiguredItemBase
    {
        private readonly QualifiedMember _sourceMember;

#if NET35
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LinqExp.LambdaExpression sourceMemberLambda)
            : this(configInfo, sourceMemberLambda.ToDlrExpression())
        {
        }
#endif
        public ConfiguredIgnoredSourceMember(MappingConfigInfo configInfo, LambdaExpression sourceMemberLambda)
            : base(configInfo)
        {
            _sourceMember = sourceMemberLambda
                                .ToSourceMemberOrNull(configInfo.MapperContext, out var failureReason) ??
                            throw new MappingConfigurationException(failureReason);
        }

        public bool CouldApplyTo(IBasicMapperData mapperData)
            => RuleSetMatches(mapperData) && TypesAreCompatible(mapperData);

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return base.AppliesTo(mapperData) &&
                   _sourceMember.LeafMember.Equals((mapperData.SourceMember as QualifiedMember)?.LeafMember);
        }
    }
}