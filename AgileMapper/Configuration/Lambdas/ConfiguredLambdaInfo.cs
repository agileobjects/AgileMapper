namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using Members.Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class ConfiguredLambdaInfo
    {
        private readonly LambdaExpression _lambda;
        private readonly Expression _lambdaBody;
        private readonly Type[] _contextTypes;
        private readonly bool _isForTargetDictionary;
        private readonly IValueReplacer _valueReplacer;
        private LambdaExpression _sourceMemberLambda;
        private bool? _isSourceMember;
        private string _description;

        private ConfiguredLambdaInfo(
            LambdaExpression lambda,
            Type[] contextTypes,
            Type returnType,
            ValueReplacerFactory valueReplacerFactory,
            MappingConfigInfo configInfo)
        {
            _lambda = lambda;
            _lambdaBody = lambda.Body;
            _contextTypes = contextTypes;
            _valueReplacer = valueReplacerFactory.CreateFor(lambda, configInfo);
            ReturnType = returnType;

            _isForTargetDictionary = (contextTypes.Length > 1) && contextTypes[1].IsDictionary();
        }

        #region Factory Methods

        public static ConfiguredLambdaInfo For(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            var funcArguments = lambda.Parameters.ProjectToArray(p => p.Type);
            var contextTypes = GetContextTypes(funcArguments);
            var valueReplacerFactory = ValueReplacerFactory.For(contextTypes, funcArguments);

            return new ConfiguredLambdaInfo(
                lambda,
                contextTypes,
                lambda.ReturnType,
                valueReplacerFactory,
                configInfo);
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

        public static ConfiguredLambdaInfo ForFunc<TFunc>(
            TFunc func,
            MappingConfigInfo configInfo,
            params Type[] argumentTypes)
        {
            return For(
                func,
                configInfo,
                argumentTypes,
                funcTypes => funcTypes.Take(funcTypes.Length - 1).ToArray(),
                funcTypes => funcTypes.Last(),
                typeof(Func<>),
                typeof(Func<,>),
                typeof(Func<,,>),
                typeof(Func<,,,>));
        }

        public static ConfiguredLambdaInfo ForAction<TAction>(
            TAction action,
            MappingConfigInfo configInfo,
            params Type[] argumentTypes)
        {
            return For(
                action,
                configInfo,
                argumentTypes,
                funcTypes => funcTypes,
                _ => typeof(void),
                typeof(Action<>),
                typeof(Action<,>),
                typeof(Action<,,>),
                typeof(Action<,,,>));
        }

        private static ConfiguredLambdaInfo For<T>(
            T func,
            MappingConfigInfo configInfo,
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
            var valueReplacerFactory = ValueReplacerFactory.For(contextTypes, funcArguments);

            if (valueReplacerFactory == null)
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
                valueReplacerFactory,
                configInfo);
        }

        #endregion

        public bool UsesMappingDataObjectParameter => _valueReplacer.HasMappingContextParameter;

        public Type ReturnType { get; }

        public bool IsSourceMember => _isSourceMember ??= TryGetSourceMember(out _);

        public bool TryGetSourceMember(out LambdaExpression sourceMemberLambda)
        {
            if (_sourceMemberLambda != null)
            {
                sourceMemberLambda = _sourceMemberLambda;
                return true;
            }

            if (_lambdaBody.NodeType != ExpressionType.MemberAccess)
            {
                return IsNotSourceMember(out sourceMemberLambda);
            }

            var memberAccesses = _lambdaBody.GetMemberAccessChain(_ => { }, out var rootExpression);

            if (memberAccesses.None())
            {
                return IsNotSourceMember(out sourceMemberLambda);
            }

            var sourceParameter = default(ParameterExpression);
            var memberAccessPath = default(Expression);

            foreach (var memberAccess in memberAccesses)
            {
                if (memberAccess.NodeType != ExpressionType.MemberAccess)
                {
                    return IsNotSourceMember(out sourceMemberLambda);
                }

                if (sourceParameter == null)
                {
                    sourceParameter = rootExpression.Type.GetOrCreateSourceParameter();
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

            _isSourceMember = true;
            return true;
        }

        private bool IsNotSourceMember(out LambdaExpression sourceLambda)
        {
            sourceLambda = null;
            _isSourceMember = false;
            return false;
        }

        public bool Supports(MappingRuleSet ruleSet)
            => ruleSet.Settings?.ExpressionIsSupported(_lambda) != false;

        public bool IsSameAs(ConfiguredLambdaInfo otherLambdaInfo)
        {
            if (otherLambdaInfo == null)
            {
                return false;
            }

            if ((_lambdaBody.NodeType == ExpressionType.Invoke) ||
                (otherLambdaInfo._lambdaBody.NodeType == ExpressionType.Invoke))
            {
                return false;
            }

            return ExpressionEvaluation.AreEquivalent(_lambdaBody, otherLambdaInfo._lambdaBody);
        }

        public string GetDescription(MappingConfigInfo configInfo)
        {
            if (_description != null)
            {
                return _description;
            }

            if (TryGetSourceMember(out var sourceMemberLambda))
            {
                return _description = sourceMemberLambda
                    .ToSourceMember(configInfo.MapperContext)
                    .GetFriendlySourcePath(configInfo);
            }

            var lambdaBody = _lambdaBody.NodeType == ExpressionType.Invoke
                ? ((InvocationExpression)_lambdaBody).Expression
                : _lambdaBody;

            return _description = lambdaBody.ToReadableString();
        }

        public Expression GetBody(IMemberMapperData mapperData)
        {
            var contextTypes = _contextTypes;

            if (_isForTargetDictionary &&
                (mapperData.TargetMember is DictionaryTargetMember dictionaryMember) &&
                (dictionaryMember.HasCompatibleType(contextTypes[1])))
            {
                contextTypes = contextTypes.CopyToArray();
                contextTypes[1] = mapperData.TargetType;
            }

            return _valueReplacer.Replace(contextTypes, mapperData);
        }
    }
}