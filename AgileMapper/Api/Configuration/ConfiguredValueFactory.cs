namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredValueFactory
    {
        #region Cached Items

        private static readonly ParameterSetOption[] _parameterSwapOptions =
        {
            new ParameterSetOption(IsMappingContextArgument, Parameters.SwapForContextParameter),
            new ParameterSetOption(IsSourceAndTargetArguments, Parameters.SwapForSourceAndTarget),
            new ParameterSetOption(IsSourceTargetAndIndexArguments, Parameters.SwapForSourceTargetAndIndex)
        };

        private static bool IsMappingContextArgument(Type sourceType, Type targetType, Type[] types)
        {
            var contextTypeArgument = types[0];

            if (!contextTypeArgument.IsGenericType ||
                (contextTypeArgument.GetGenericTypeDefinition() != typeof(ITypedMemberMappingContext<,>)))
            {
                return false;
            }

            var contextTypes = contextTypeArgument.GetGenericArguments();

            return IsSourceAndTargetArguments(sourceType, targetType, contextTypes);
        }

        private static bool IsSourceAndTargetArguments(Type sourceType, Type targetType, Type[] types)
        {
            return sourceType.IsAssignableFrom(types[0]) && targetType.IsAssignableFrom(types[1]);
        }

        private static bool IsSourceTargetAndIndexArguments(Type sourceType, Type targetType, Type[] types)
        {
            return IsSourceAndTargetArguments(sourceType, targetType, types) &&
                   typeof(int?).IsAssignableFrom(types[2]);
        }

        #endregion

        private ConfiguredValueFactory(
            LambdaExpression lambda,
            Type returnType,
            Func<LambdaExpression, IMemberMappingContext, Expression> parametersSwapper)
        {
            Lambda = lambda;
            ReturnType = returnType;
            ParametersSwapper = parametersSwapper;
        }

        #region Factory Method

        public static ConfiguredValueFactory For<TValue>(TValue value, Type sourceType, Type targetType)
        {
            var funcType = typeof(TValue);

            if (!funcType.IsGenericType)
            {
                return null;
            }

            var funcTypeDefinition = funcType.GetGenericTypeDefinition();

            if ((funcTypeDefinition != typeof(Func<,>)) &&
                (funcTypeDefinition != typeof(Func<,,>)) &&
                (funcTypeDefinition != typeof(Func<,,,>)))
            {
                return null;
            }

            var funcTypes = funcType.GetGenericArguments();
            var parameterSwapOption = _parameterSwapOptions.ElementAt(funcTypes.Length - 2);
            var funcArguments = funcTypes.Take(funcTypes.Length - 1).ToArray();

            if (!parameterSwapOption.AppliesTo(sourceType, targetType, funcArguments))
            {
                return null;
            }

            var parameters = funcArguments.Select(Parameters.Create).ToArray();
            var valueFactory = Expression.Constant(value, funcType);
            var valueFactoryInvocation = Expression.Invoke(valueFactory, parameters.Cast<Expression>());
            var valueFactoryLambda = Expression.Lambda(funcType, valueFactoryInvocation, parameters);

            return new ConfiguredValueFactory(
                valueFactoryLambda,
                funcTypes.Last(),
                parameterSwapOption.ParametersSwapper);
        }

        #endregion

        public LambdaExpression Lambda { get; }

        public Type ReturnType { get; }

        public Func<LambdaExpression, IMemberMappingContext, Expression> ParametersSwapper { get; }

        private class ParameterSetOption
        {
            private readonly Func<Type, Type, Type[], bool> _applicabilityPredicate;

            public ParameterSetOption(
                Func<Type, Type, Type[], bool> applicabilityPredicate,
                Func<LambdaExpression, IMemberMappingContext, Expression> parametersSwapper)
            {
                _applicabilityPredicate = applicabilityPredicate;
                ParametersSwapper = parametersSwapper;
            }

            public Func<LambdaExpression, IMemberMappingContext, Expression> ParametersSwapper { get; }

            public bool AppliesTo(Type sourceType, Type targetType, Type[] funcArguments)
                => _applicabilityPredicate.Invoke(sourceType, targetType, funcArguments);
        }
    }
}