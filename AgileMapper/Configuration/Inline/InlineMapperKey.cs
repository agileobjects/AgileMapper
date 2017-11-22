namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Extensions;
    using Members;

    internal class InlineMapperKey<TSource, TTarget> : IInlineMapperKey
    {
        private readonly Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] _configurations;
        private readonly MappingExecutor<TSource> _executor;

        public InlineMapperKey(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations,
            MappingExecutor<TSource> executor)
        {
            _configurations = configurations;
            _executor = executor;
        }

        public MappingTypes MappingTypes => MappingTypes<TSource, TTarget>.Fixed;

        public MappingRuleSet RuleSet => _executor.RuleSet;

        // ReSharper disable once CoVariantArrayConversion
        public IList<LambdaExpression> Configurations => _configurations;

        public MapperContext CreateInlineMapperContext()
        {
            return InlineMappingConfigurator<TSource, TTarget>
                .ConfigureInlineMapperContext(_configurations, _executor);
        }

        public override bool Equals(object obj)
        {
            var otherKey = (IInlineMapperKey)obj;

            if ((_configurations.Length != otherKey.Configurations.Count) ||
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