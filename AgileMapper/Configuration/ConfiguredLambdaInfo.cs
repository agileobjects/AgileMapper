﻿namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
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
        private LambdaExpression _sourceMemberLambda;
        private string _description;

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
            var funcArguments = lambda.Parameters.ProjectToArray(p => p.Type);
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

            var firstArgument = funcArguments[0];

            if (firstArgument.IsGenericType() &&
                firstArgument.IsAssignableTo(typeof(IServiceProviderAccessor)))
            {
                return firstArgument.GetGenericTypeArguments();
            }

            return new[] { firstArgument };
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

            var parameters = funcArguments.ProjectToArray(Parameters.Create);
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

        public bool IsSourceMember(out LambdaExpression sourceMemberLambda)
        {
            if (_lambda.Body.NodeType != ExpressionType.MemberAccess)
            {
                sourceMemberLambda = null;
                return false;
            }

            if (_sourceMemberLambda != null)
            {
                sourceMemberLambda = _sourceMemberLambda;
                return true;
            }

            var memberAccesses = _lambda.Body.GetMemberAccessChain(nt => { }, out var rootExpression);

            if (memberAccesses == null)
            {
                sourceMemberLambda = null;
                return false;
            }

            var sourceParameter = default(ParameterExpression);
            var memberAccessPath = default(Expression);

            foreach (var memberAccess in memberAccesses)
            {
                if (memberAccess.NodeType != ExpressionType.MemberAccess)
                {
                    sourceMemberLambda = null;
                    return false;
                }

                if (sourceParameter == null)
                {
                    sourceParameter = Parameters.Create(rootExpression.Type, "source");
                    memberAccessPath = sourceParameter;
                }

                memberAccessPath = Expression.MakeMemberAccess(
                    memberAccessPath,
                  ((MemberExpression)memberAccess).Member);
            }

            // ReSharper disable PossibleNullReferenceException
            _sourceMemberLambda = sourceMemberLambda = Expression.Lambda(
                Expression.GetFuncType(sourceParameter.Type, memberAccessPath.Type),
                memberAccessPath,
                sourceParameter);
            // ReSharper restore PossibleNullReferenceException

            return true;
        }

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

        public string GetDescription(MappingConfigInfo configInfo)
        {
            if (_description != null)
            {
                return _description;
            }

            if (IsSourceMember(out var sourceMemberLambda))
            {
                return _description = sourceMemberLambda
                    .ToSourceMember(configInfo.MapperContext)
                    .GetFriendlySourcePath(configInfo);
            }

            return _description = _lambda.Body.ToReadableString();
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