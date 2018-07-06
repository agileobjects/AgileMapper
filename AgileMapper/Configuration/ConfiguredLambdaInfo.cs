namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;
    using ObjectPopulation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredLambdaInfo
    {
        private readonly LambdaExpression _lambda;
        private readonly Type[] _contextTypes;
        private readonly bool _isForTargetDictionary;
        private readonly ParametersSwapper _parametersSwapper;

        private ConfiguredLambdaInfo(
            LambdaExpression lambda,
            Type[] contextTypes,
            Type returnType,
            ParametersSwapper parametersSwapper)
        {
            _lambda = lambda;
            _contextTypes = contextTypes;
            _parametersSwapper = parametersSwapper;
            ReturnType = returnType;

            _isForTargetDictionary = (contextTypes.Length > 1) && contextTypes[1].IsDictionary();
        }

        #region Factory Methods

        public static ConfiguredLambdaInfo For(LambdaExpression lambda)
        {
            var funcArguments = lambda.Parameters.Project(p => p.Type).ToArray();
            var contextTypes = GetContextTypes(funcArguments);
            var parameterSwapper = ParametersSwapper.For(contextTypes, funcArguments);

            return new ConfiguredLambdaInfo(lambda, contextTypes, lambda.ReturnType, parameterSwapper);
        }

        private static Type[] GetContextTypes(Type[] funcArguments)
        {
            if (funcArguments.Length != 1)
            {
                return funcArguments;
            }

            if (funcArguments[0].IsGenericType())
            {
                return funcArguments[0].GetGenericTypeArguments();
            }

            return new[] { funcArguments[0] };
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

            var parameters = funcArguments.Project(Parameters.Create).ToArray();
            var valueFactory = func.ToConstantExpression();
            var valueFactoryInvocation = Expression.Invoke(valueFactory, parameters.Cast<Expression>());
            var valueFactoryLambda = Expression.Lambda(funcType, valueFactoryInvocation, parameters);

            return new ConfiguredLambdaInfo(
                valueFactoryLambda,
                contextTypes,
                returnTypeFactory.Invoke(funcTypes),
                parameterSwapper);
        }

        #endregion

        public bool UsesMappingDataObjectParameter => _parametersSwapper.HasMappingContextParameter;

        public Type ReturnType { get; }

        public bool Supports(MappingRuleSet ruleSet)
            => ruleSet.Settings?.ExpressionIsSupported(_lambda) != false;

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
            var contextTypes = _contextTypes;

            if (_isForTargetDictionary &&
                (mapperData.TargetMember is DictionaryTargetMember dictionaryMember) &&
                (dictionaryMember.HasCompatibleType(contextTypes[1])))
            {
                contextTypes = contextTypes.ToArray();
                contextTypes[1] = mapperData.TargetType;
            }

            return position.IsPriorToObjectCreation(targetMember)
                ? _parametersSwapper.Swap(_lambda, contextTypes, mapperData, ParametersSwapper.UseTargetMember)
                : _parametersSwapper.Swap(_lambda, contextTypes, mapperData, ParametersSwapper.UseTargetInstance);
        }
    }
}