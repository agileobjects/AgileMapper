namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Configuration;
    using Extensions;
    using Members;

    internal class InlineMapperKey<TSource, TTarget> : IInlineMapperKey
    {
        private readonly Expression<Action<IFullMappingConfigurator<TSource, TTarget>>>[] _configurations;
        private readonly MappingExecutor<TSource> _initiatingExecutor;

        public InlineMapperKey(
            Expression<Action<IFullMappingConfigurator<TSource, TTarget>>>[] configurations,
            MappingExecutor<TSource> initiatingExecutor)
        {
            _configurations = configurations;
            _initiatingExecutor = initiatingExecutor;
        }

        public MappingTypes MappingTypes => MappingTypes<TSource, TTarget>.Fixed;

        public MappingRuleSet RuleSet => _initiatingExecutor.RuleSet;

        // ReSharper disable once CoVariantArrayConversion
        public IList<LambdaExpression> Configurations => _configurations;

        public MulticastDelegate CreateExecutor()
        {
            var sourceParameter = Parameters.Create<TSource>("source");
            var targetParameter = Parameters.Create<TTarget>("target");

            var executorCall = GetNewExecutorCall(sourceParameter, targetParameter);

            var executorLambda = Expression.Lambda<InlineMappingExecutor<TSource, TTarget>>(
                executorCall,
                sourceParameter,
                targetParameter);

            return executorLambda.Compile();
        }

        private Expression GetNewExecutorCall(Expression source, Expression target)
        {
            var ruleSet = _initiatingExecutor
                .RuleSet
                .ToConstantExpression();

            var inlineMapperContext = CreateConfiguredInlineMapperContext();

            var mergedInlineMapperContext = _initiatingExecutor
                .MapperContext
                .Clone()
                .Merge(inlineMapperContext)
                .ToConstantExpression();

            var newExecutor = Expression.New(
                typeof(MappingExecutor<TSource>).GetConstructors().Last(),
                source,
                ruleSet,
                mergedInlineMapperContext);

            return GetExecutorCall(newExecutor, target);
        }

        private MapperContext CreateConfiguredInlineMapperContext()
        {
            var inlineMapperContext = new MapperContext();

            var configInfo = new MappingConfigInfo(inlineMapperContext)
                .ForRuleSet(_initiatingExecutor.RuleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            var configurator = new MappingConfigurator<TSource, TTarget>(
                configInfo);

            foreach (var configuration in _configurations)
            {
                configuration.Compile().Invoke(configurator);
            }

            return inlineMapperContext;
        }

        private static Expression GetExecutorCall(Expression executor, Expression target)
        {
            var performMappingMethod = executor
                .Type
                .GetMethod("PerformMapping")
                .MakeGenericMethod(typeof(TTarget));

            return Expression.Call(
                executor,
                performMappingMethod,
                target);
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