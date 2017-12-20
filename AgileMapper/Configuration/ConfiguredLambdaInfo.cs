namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class ConfiguredLambdaInfo
    {
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
            var contextTypes = (funcArguments.Length != 1) ? funcArguments : funcArguments[0].GetGenericTypeArguments();
            var parameterSwapper = ParametersSwapper.For(contextTypes, funcArguments);

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

            if (!funcType.IsGenericType())
            {
                return null;
            }

            var funcTypeDefinition = funcType.GetGenericTypeDefinition();

            if (!allowedTypes.Contains(funcTypeDefinition))
            {
                return null;
            }

            var funcTypes = funcType.GetGenericTypeArguments();
            var funcArguments = funcArgumentsFactory.Invoke(funcTypes);
            var parameterSwapper = ParametersSwapper.For(contextTypes, funcArguments);

            if (parameterSwapper == null)
            {
                return null;
            }

            var parameters = funcArguments.Select(Parameters.Create).ToArray();
            var valueFactory = func.ToConstantExpression();
            var valueFactoryInvocation = Expression.Invoke(valueFactory, parameters.Cast<Expression>());
            var valueFactoryLambda = Expression.Lambda(funcType, valueFactoryInvocation, parameters);

            return new ConfiguredLambdaInfo(
                valueFactoryLambda,
                returnTypeFactory.Invoke(funcTypes),
                parameterSwapper);
        }

        #endregion

        public bool UsesMappingDataObjectParameter => _parametersSwapper.NumberOfParameters == 1;

        public Type ReturnType { get; }

        public bool IsSameAs(ConfiguredLambdaInfo otherLambdaInfo)
        {
            if (otherLambdaInfo == null)
            {
                return false;
            }

            if ((_lambda.Body.NodeType == ExpressionType.Invoke) ||
                (otherLambdaInfo._lambda.Body.NodeType == ExpressionType.Invoke))
            {
                return false;
            }

            return ExpressionEvaluation.AreEquivalent(_lambda.Body, otherLambdaInfo._lambda.Body);
        }

        public Expression GetBody(
            IMemberMapperData mapperData,
            CallbackPosition? position = null,
            QualifiedMember targetMember = null)
        {
            return position.IsPriorToObjectCreation(targetMember)
                ? _parametersSwapper.Swap(_lambda, mapperData, ParametersSwapper.UseTargetMember)
                : _parametersSwapper.Swap(_lambda, mapperData, ParametersSwapper.UseTargetInstance);
        }
    }
}