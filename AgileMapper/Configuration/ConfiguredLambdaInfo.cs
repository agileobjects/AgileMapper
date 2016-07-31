namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredLambdaInfo
    {
        #region Cached Items

        private static readonly ParametersSwapper[] _parameterSwappers =
        {
            new ParametersSwapper(0, (ct, ft) => true, Parameters.SwapNothing),
            new ParametersSwapper(1, IsMemberMappingContext, Parameters.SwapForContextParameter),
            new ParametersSwapper(1, IsObjectCreationContext, Parameters.SwapForContextParameter),
            new ParametersSwapper(2, IsSourceAndTarget, Parameters.SwapForSourceAndTarget),
            new ParametersSwapper(3, IsSourceTargetAndIndex, Parameters.SwapForSourceTargetAndIndex),
            new ParametersSwapper(3, IsSourceTargetAndInstance, Parameters.SwapForSourceTargetAndInstance),
            new ParametersSwapper(4, IsSourceTargetInstanceAndIndex, Parameters.SwapForSourceTargetInstanceAndIndex)
        };

        private static bool IsMemberMappingContext(Type[] contextTypes, Type[] funcArguments)
            => Is(typeof(ITypedMemberMappingContext<,>), contextTypes, funcArguments, IsSourceAndTarget);

        private static bool IsObjectCreationContext(Type[] contextTypes, Type[] funcArguments)
            => Is(typeof(IObjectCreationContext<,,>), contextTypes, funcArguments, IsSourceTargetAndInstance);

        private static bool Is(
            Type contextType,
            Type[] contextTypes,
            IList<Type> funcArguments,
            Func<Type[], Type[], bool> parametersChecker)
        {
            var contextTypeArgument = funcArguments[0];

            if (!contextTypeArgument.IsGenericType)
            {
                return false;
            }

            var contextGenericDefinition = contextTypeArgument.GetGenericTypeDefinition();

            if (contextGenericDefinition != contextType)
            {
                return false;
            }

            return parametersChecker.Invoke(contextTypes, contextTypeArgument.GetGenericArguments());
        }

        private static bool IsSourceAndTarget(Type[] contextTypes, Type[] funcArguments)
            => funcArguments[0].IsAssignableFrom(contextTypes[0]) && funcArguments[1].IsAssignableFrom(contextTypes[1]);

        private static bool IsSourceTargetAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && IsIndex(funcArguments);

        private static bool IsIndex(IEnumerable<Type> funcArguments) => funcArguments.Last() == typeof(int?);

        private static bool IsSourceTargetAndInstance(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && (contextTypes.Length >= 3) && funcArguments[2].IsAssignableFrom(contextTypes[2]);

        private static bool IsSourceTargetInstanceAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceTargetAndInstance(contextTypes, funcArguments) && IsIndex(funcArguments);

        #endregion

        private readonly LambdaExpression _lambda;
        private readonly ParametersSwapper _parametersSwapper;

        private ConfiguredLambdaInfo(
            LambdaExpression lambda,
            Type returnType,
            ParametersSwapper parametersSwapper)
        {
            _lambda = lambda;
            _parametersSwapper = parametersSwapper;
            ReturnType = returnType;
        }

        #region Factory Methods

        public static ConfiguredLambdaInfo For(LambdaExpression lambda)
        {
            var funcArguments = lambda.Parameters.Select(p => p.Type).ToArray();
            var contextTypes = (funcArguments.Length != 1) ? funcArguments : funcArguments[0].GetGenericArguments();
            var parameterSwapper = GetParametersSwapperFor(contextTypes, funcArguments);

            return new ConfiguredLambdaInfo(lambda, lambda.ReturnType, parameterSwapper);
        }

        public static ConfiguredLambdaInfo ForFunc<TFunc>(TFunc func, params Type[] argumentTypes)
        {
            return For(
                func,
                argumentTypes,
                funcTypes => funcTypes.Take(funcTypes.Length - 1).ToArray(),
                funcTypes => funcTypes.Last(),
                typeof(Func<>),
                typeof(Func<,>),
                typeof(Func<,,>),
                typeof(Func<,,,>));
        }

        public static ConfiguredLambdaInfo ForAction<TAction>(TAction action, params Type[] argumentTypes)
        {
            return For(
                action,
                argumentTypes,
                funcTypes => funcTypes,
                funcTypes => typeof(void),
                typeof(Action<>),
                typeof(Action<,>),
                typeof(Action<,,>),
                typeof(Action<,,,>));
        }

        private static ConfiguredLambdaInfo For<T>(
            T func,
            Type[] contextTypes,
            Func<Type[], Type[]> funcArgumentsFactory,
            Func<Type[], Type> returnTypeFactory,
            params Type[] allowedTypes)
        {
            var funcType = typeof(T);

            if (!funcType.IsGenericType)
            {
                return null;
            }

            var funcTypeDefinition = funcType.GetGenericTypeDefinition();

            if (!allowedTypes.Contains(funcTypeDefinition))
            {
                return null;
            }

            var funcTypes = funcType.GetGenericArguments();
            var funcArguments = funcArgumentsFactory.Invoke(funcTypes);
            var parameterSwapper = GetParametersSwapperFor(contextTypes, funcArguments);

            if (parameterSwapper == null)
            {
                return null;
            }

            var parameters = funcArguments.Select(Parameters.Create).ToArray();
            var valueFactory = Expression.Constant(func, funcType);
            var valueFactoryInvocation = Expression.Invoke(valueFactory, parameters.Cast<Expression>());
            var valueFactoryLambda = Expression.Lambda(funcType, valueFactoryInvocation, parameters);

            return new ConfiguredLambdaInfo(
                valueFactoryLambda,
                returnTypeFactory.Invoke(funcTypes),
                parameterSwapper);
        }

        private static ParametersSwapper GetParametersSwapperFor(Type[] contextTypes, Type[] funcArguments)
            => _parameterSwappers.FirstOrDefault(pso => pso.AppliesTo(contextTypes, funcArguments));

        #endregion

        public Type ReturnType { get; }

        public bool IsSameAs(ConfiguredLambdaInfo otherLambdaInfo)
            => _lambda.ToString() == otherLambdaInfo._lambda.ToString();

        public Expression GetBody(IMemberMappingContext context) => _parametersSwapper.Swap(_lambda, context);

        private class ParametersSwapper
        {
            private readonly int _numberOfParameters;
            private readonly Func<Type[], Type[], bool> _applicabilityPredicate;
            private readonly Func<LambdaExpression, IMemberMappingContext, Expression> _parametersSwapper;

            public ParametersSwapper(
                int numberOfParameters,
                Func<Type[], Type[], bool> applicabilityPredicate,
                Func<LambdaExpression, IMemberMappingContext, Expression> parametersSwapper)
            {
                _numberOfParameters = numberOfParameters;
                _applicabilityPredicate = applicabilityPredicate;
                _parametersSwapper = parametersSwapper;
            }

            public bool AppliesTo(Type[] contextTypes, Type[] funcArguments)
                => (funcArguments.Length == _numberOfParameters) && _applicabilityPredicate.Invoke(contextTypes, funcArguments);

            public Expression Swap(LambdaExpression lambda, IMemberMappingContext context)
                => _parametersSwapper.Invoke(lambda, context);
        }
    }
}