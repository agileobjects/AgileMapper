namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;

    internal class InlineMapperKey<TSource, TTarget> : IInlineMapperKey
    {
        private readonly IInlineConfigurationSet _configurations;
        private readonly MappingExecutor<TSource> _executor;

        public InlineMapperKey(IInlineConfigurationSet configurations, MappingExecutor<TSource> executor)
        {
            _configurations = configurations;
            _executor = executor;
        }

        public MappingTypes MappingTypes => MappingTypes<TSource, TTarget>.Fixed;

        public MappingRuleSet RuleSet => _executor.RuleSet;

        // ReSharper disable once CoVariantArrayConversion
        public IList<LambdaExpression> Configurations => _configurations.Lambdas;

        public MapperContext CreateInlineMapperContext()
        {
            var inlineMapperContext = _executor.MapperContext.Clone();

            _configurations.Apply(_executor.RuleSet, inlineMapperContext);

            return inlineMapperContext;
        }

        public override bool Equals(object obj)
        {
            var otherKey = (IInlineMapperKey)obj;

            if ((_configurations.Count != otherKey.Configurations.Count) ||
                (RuleSet != otherKey.RuleSet) ||
                !MappingTypes.Equals(otherKey.MappingTypes))
            {
                return false;
            }

            for (var i = 0; i < Configurations.Count; i++)
            {
                var configuration = Configurations[i].Body;
                var otherConfiguration = otherKey.Configurations[i].Body;

                if (!ExpressionEquator.Instance.Equals(configuration, otherConfiguration))
                {
                    return false;
                }
            }

            return true;
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}