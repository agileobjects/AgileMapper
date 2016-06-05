namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class TargetTypeSpecifier<TSource>
    {
        private readonly MappingConfigInfo _configInfo;

        internal TargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IFullMappingConfigurator<TSource, TTarget> To<TTarget>() where TTarget : class
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForAllRuleSets());

        public IFullMappingConfigurator<TSource, TTarget> ToANew<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.CreateNew);

        public IFullMappingConfigurator<TSource, TTarget> OnTo<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.Merge);

        public IFullMappingConfigurator<TSource, TTarget> Over<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.Overwrite);

        private MappingConfigurator<TSource, TTarget> UsingRuleSet<TTarget>(string name)
            where TTarget : class
        {
            return new MappingConfigurator<TSource, TTarget>(_configInfo.ForRuleSet(name));
        }
    }
}