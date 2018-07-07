namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class InlineMapperKey<TSource, TTarget, TConfigurator> : IInlineMapperKey
    {
        private readonly Expression<Action<TConfigurator>>[] _configurations;
        private readonly Func<MappingConfigInfo, TConfigurator> _configuratorFactory;
        private readonly IMappingContext _mappingContext;

        public InlineMapperKey(
            Expression<Action<TConfigurator>>[] configurations,
            Func<MappingConfigInfo, TConfigurator> configuratorFactory,
            IMappingContext mappingContext)
        {
            _configurations = configurations;
            _configuratorFactory = configuratorFactory;
            _mappingContext = mappingContext;
        }

        public MappingTypes MappingTypes => MappingTypes<TSource, TTarget>.Fixed;

        public MappingRuleSet RuleSet => _mappingContext.RuleSet;

        public Type ConfiguratorType => typeof(TConfigurator);

        // ReSharper disable once CoVariantArrayConversion
        public IList<LambdaExpression> Configurations => _configurations;

        public MapperContext CreateInlineMapperContext()
        {
            return InlineMappingConfigurator<TSource, TTarget>
                .ConfigureInlineMapperContext(
                    _configurations,
                    _configuratorFactory,
                    _mappingContext);
        }

        public override bool Equals(object obj)
        {
            var otherKey = (IInlineMapperKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            if ((_configurations.Length != otherKey.Configurations.Count) ||
                (ConfiguratorType != otherKey.ConfiguratorType) ||
                (RuleSet != otherKey.RuleSet) ||
                !MappingTypes.Equals(otherKey.MappingTypes))
            {
                return false;
            }

            for (var i = 0; i < Configurations.Count; i++)
            {
                var configuration = Configurations[i].Body;
                var otherConfiguration = otherKey.Configurations[i].Body;

                if (!ExpressionEvaluation.AreEquivalent(configuration, otherConfiguration))
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