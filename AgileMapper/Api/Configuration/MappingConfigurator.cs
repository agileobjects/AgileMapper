namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    public class MappingConfigurator<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal MappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression,
                Parameters.SwapForContextParameter);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression,
                Parameters.SwapForSourceAndTarget);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression,
                Parameters.SwapForSourceTargetAndIndex);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(Func<TSource, TSourceValue> valueFunc)
        {
            return GetConstantTargetMemberSpecifier(valueFunc);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
        {
            LambdaExpression valueFactoryLambda;
            Type valueFactoryReturnType;

            return TryGetValueFactory(value, out valueFactoryLambda, out valueFactoryReturnType)
                ? new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                    _configInfo.ForSourceValueType(valueFactoryReturnType),
                    valueFactoryLambda,
                    Parameters.SwapForContextParameter)
                : GetConstantTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private static bool TryGetValueFactory<TSourceValue>(
            TSourceValue value,
            out LambdaExpression valueFactoryLambda,
            out Type valueFactoryReturnType)
        {
            if (typeof(TSourceValue).IsGenericType &&
                (typeof(TSourceValue).GetGenericTypeDefinition() == typeof(Func<,>)))
            {
                var funcTypeArguments = typeof(TSourceValue).GetGenericArguments();
                var contextTypeArgument = funcTypeArguments.First();

                if (contextTypeArgument.IsGenericType &&
                    (contextTypeArgument.GetGenericTypeDefinition() == typeof(ITypedMemberMappingContext<,>)))
                {
                    var contextTypes = contextTypeArgument.GetGenericArguments();

                    if (typeof(TSource).IsAssignableFrom(contextTypes.First()))
                    {
                        var parameters = funcTypeArguments
                            .Take(funcTypeArguments.Length - 1)
                            .Select(Parameters.Create)
                            .ToArray();

                        var valueFactory = Expression.Constant(value, typeof(TSourceValue));
                        var valueFactoryInvocation = Expression.Invoke(valueFactory, parameters.Cast<Expression>());
                        valueFactoryLambda = Expression.Lambda(typeof(TSourceValue), valueFactoryInvocation, parameters);

                        valueFactoryReturnType = funcTypeArguments.Last();
                        return true;
                    }
                }
            }

            valueFactoryLambda = null;
            valueFactoryReturnType = null;
            return false;
        }

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantTargetMemberSpecifier<TSourceValue>(TSourceValue value)
        {
            var valueConstant = Expression.Constant(value, typeof(TSourceValue));
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(valueConstant.Type),
                valueLambda,
                Parameters.SwapNothing);
        }

        #endregion

        public ConditionSpecifier<TSource, TTarget> Ignore<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            var configuredIgnoredMember = ConfiguredIgnoredMember.For(
                _configInfo,
                typeof(TTarget),
                targetMember.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredIgnoredMember);

            return new ConditionSpecifier<TSource, TTarget>(configuredIgnoredMember, negateCondition: true);
        }

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before => new PreEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After => new PostEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);
    }
}