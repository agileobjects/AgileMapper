namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Configuration;
    using Extensions;
    using Members;

#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif

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
            var executorParameter = Parameters.Create<MappingExecutor<TSource>>("executor");
            var sourceParameter = Parameters.Create<TSource>("source");
            var targetParameter = Parameters.Create<TTarget>("target");

            var inlineMapperContext = CreateConfiguredInlineMapperContext();

            Expression executorCall;

            if (_initiatingExecutor.MapperContext.ConflictsWith(inlineMapperContext))
            {
                var ruleSet = _initiatingExecutor.RuleSet.ToConstantExpression();
                //var mapperContext = mergedCloneMapperContext.ToConstantExpression();
                //var inlineMappingExecutor = new MappingExecutor<TSource>(, mergedCloneMapperContext);

                executorCall = null;
            }
            else
            {
                _initiatingExecutor.MapperContext.Merge(inlineMapperContext);

                // The MapperContext on the initiating MappingExecutor is compatible with the 
                // supplied inline configurations. We can therefore just call PerformMapping
                // to continue the mapping on the initiating MappingExecutor:
                executorCall = GetInitiatingExecutorCall(executorParameter, targetParameter);
            }

            var executorLambda = Expression.Lambda<InlineMappingExecutor<TSource, TTarget>>(
                executorCall,
                sourceParameter,
                targetParameter,
                executorParameter);

            return executorLambda.Compile();
        }

        private MapperContext CreateConfiguredInlineMapperContext()
        {
            var inlineMapperContext = new MapperContext();

            var configInfo = new MappingConfigInfo(inlineMapperContext)
                .ForRuleSet(_initiatingExecutor.RuleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            var configurator = new MappingConfigurator<TSource, TTarget>(configInfo);

            foreach (var configuration in _configurations)
            {
                configuration.Compile().Invoke(configurator);
            }

            return inlineMapperContext;
        }

        private static Expression GetInitiatingExecutorCall(Expression executorParameter, Expression targetParameter)
        {
            var performMappingMethod = executorParameter
                .Type
                .GetMethod("PerformMapping")
                .MakeGenericMethod(typeof(TTarget));

            return Expression.Call(
                executorParameter,
                performMappingMethod,
                targetParameter);
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
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}