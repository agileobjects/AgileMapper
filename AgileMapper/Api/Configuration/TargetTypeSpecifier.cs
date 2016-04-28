namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class TargetTypeSpecifier<TSource>
    {
        private readonly MappingConfigInfo _configInfo;

        internal TargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public MappingConfigurator<TSource, TTarget> To<TTarget>()
            where TTarget : class
        {
            return new MappingConfigurator<TSource, TTarget>(_configInfo.ForAllRuleSets());
        }

        public MappingConfigurator<TSource, TTarget> ToANew<TTarget>()
            where TTarget : class
        {
            return UsingRuleSet<TTarget>(Constants.CreateNew);
        }

        public MappingConfigurator<TSource, TTarget> OnTo<TTarget>()
            where TTarget : class
        {
            return UsingRuleSet<TTarget>(Constants.Merge);
        }

        public MappingConfigurator<TSource, TTarget> Over<TTarget>()
            where TTarget : class
        {
            return UsingRuleSet<TTarget>(Constants.Overwrite);
        }

        private MappingConfigurator<TSource, TTarget> UsingRuleSet<TTarget>(string name)
            where TTarget : class
        {
            return new MappingConfigurator<TSource, TTarget>(_configInfo.ForRuleSet(name));
        }
    }
}