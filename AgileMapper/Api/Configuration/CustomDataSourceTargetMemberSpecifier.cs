namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions.Extensions;


    internal class CustomDataSourceTargetMemberSpecifier<TSource, TTarget> :
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget>,
        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _customValueLambda;
        private readonly ConfiguredLambdaInfo _customValueLambdaInfo;

        public CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            LambdaExpression customValueLambda)
            : this(configInfo, default(ConfiguredLambdaInfo))
        {
            _customValueLambda = customValueLambda;
        }

        public CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo customValueLambda)
        {
            _configInfo = configInfo;
            _customValueLambdaInfo = customValueLambda;
        }


        public IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            return RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetMember));
        }

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.To<TResultValue>(
            Expression<Func<TTarget, TResultValue>> resultMember)
        {
            return RegisterDataSource<TResultValue>(() => CreateFromLambda<TResultValue>(resultMember));
        }

        public IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetSetMethod));

        private ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            var valueLambda = GetValueLambda<TTargetValue>();

            if (IsDictionaryEntry(targetMemberLambda, out var dictionaryEntryMember))
            {
                return new ConfiguredDictionaryDataSourceFactory(_configInfo, valueLambda, dictionaryEntryMember);
            }

            return new ConfiguredDataSourceFactory(
                _configInfo,
                valueLambda,
                targetMemberLambda);
        }

        private bool IsDictionaryEntry(LambdaExpression targetMemberLambda, out DictionaryTargetMember entryMember)
        {
            if (targetMemberLambda.Body.NodeType != ExpressionType.Call)
            {
                entryMember = null;
                return false;
            }

            var methodCall = (MethodCallExpression)targetMemberLambda.Body;

            if (!methodCall.Method.IsSpecialName ||
                (methodCall.Method.Name != "get_Item") ||
                !methodCall.Method.DeclaringType.IsDictionary())
            {
                // TODO: Test coverage - specified, non-dictionary indexed target member
                entryMember = null;
                return false;
            }

            var entryKeyExpression = methodCall.Arguments[0];

            if (entryKeyExpression.NodeType != ExpressionType.Constant)
            {
                throw new MappingConfigurationException(
                    "Target dictionary keys must be constant string values.");
            }

            var entryKey = (string)((ConstantExpression)entryKeyExpression).Value;

            var memberFactory = _configInfo.MapperContext.QualifiedMemberFactory;

            var rootMember = (DictionaryTargetMember)(_configInfo.TargetType == typeof(ExpandoObject)
                ? memberFactory.RootTarget<TSource, ExpandoObject>()
                : memberFactory.RootTarget<TSource, TTarget>());

            entryMember = rootMember.Append(typeof(TSource), entryKey);
            return true;
        }

        private ConfiguredLambdaInfo GetValueLambda<TTargetValue>()
        {
            if (_customValueLambdaInfo != null)
            {
                return _customValueLambdaInfo;
            }

            if ((_customValueLambda.Body.NodeType != ExpressionType.Constant) ||
                (typeof(TTargetValue) == typeof(object)) ||
                _customValueLambda.ReturnType.IsAssignableTo(typeof(TTargetValue)))
            {
                return ConfiguredLambdaInfo.For(_customValueLambda);
            }

            var convertedConstantValue = _configInfo
                .MapperContext
                .ValueConverters
                .GetConversion(_customValueLambda.Body, typeof(TTargetValue));

            var valueLambda = Expression.Lambda<Func<TTargetValue>>(convertedConstantValue);
            var valueFunc = valueLambda.Compile();
            var value = valueFunc.Invoke().ToConstantExpression(typeof(TTargetValue));
            var constantValueLambda = Expression.Lambda<Func<TTargetValue>>(value);
            var valueLambdaInfo = ConfiguredLambdaInfo.For(constantValueLambda);

            return valueLambdaInfo;
        }

        public IMappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        public IMappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName)
            => RegisterDataSource<object>(() => CreateForCtorParam(parameterName));

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor(
            string parameterName)
        {
            return RegisterDataSource<object>(() => CreateForCtorParam(parameterName));
        }

        #region Ctor Helpers

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>()
            => CreateForCtorParam<TParam>(GetUniqueConstructorParameterOrThrow<TParam>());

        private ConfiguredDataSourceFactory CreateForCtorParam(string name)
            => CreateForCtorParam<object>(GetUniqueConstructorParameterOrThrow<AnyParameterType>(name));

        private static ParameterInfo GetUniqueConstructorParameterOrThrow<TParam>(string name = null)
        {
            var ignoreParameterType = typeof(TParam) == typeof(AnyParameterType);
            var ignoreParameterName = name == null;

            var matchingParameters = typeof(TTarget)
                .GetPublicInstanceConstructors()
                .Select(ctor => new
                {
                    Ctor = ctor,
                    MatchingParameters = ctor
                        .GetParameters()
                        .Where(p =>
                            (ignoreParameterType || (p.ParameterType == typeof(TParam))) &&
                            (ignoreParameterName || (p.Name == name)))
                        .ToArray()
                })
                .Where(d => d.MatchingParameters.Any())
                .ToArray();

            if (matchingParameters.Length == 0)
            {
                throw MissingParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameterData = matchingParameters.First();

            if (matchingParameterData.MatchingParameters.Length > 1)
            {
                throw AmbiguousParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameter = matchingParameterData.MatchingParameters.First();

            return matchingParameter;
        }

        private static string GetParameterMatchInfo<TParam>(string name, bool matchParameterType)
            => matchParameterType ? "of type " + typeof(TParam).GetFriendlyName() : "named '" + name + "'";

        private static Exception MissingParameterException(string parameterMatchInfo)
        {
            return new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "No constructor parameter {0} exists on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private static Exception AmbiguousParameterException(string parameterMatchInfo)
        {
            return new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Multiple constructor parameters found {0} on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>(ParameterInfo parameter)
        {
            var memberChain = new[]
            {
                Member.RootTarget<TTarget>(),
                Member.ConstructorParameter(parameter)
            };

            var valueLambda = GetValueLambda<TParam>();
            var constructorParameter = QualifiedMember.From(memberChain, _configInfo.MapperContext);

            return new ConfiguredDataSourceFactory(_configInfo, valueLambda, constructorParameter);
        }

        #endregion

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            Func<ConfiguredDataSourceFactory> factoryFactory)
        {
            _configInfo.ThrowIfSourceTypeUnconvertible<TTargetValue>();
            _configInfo.MapperContext.UserConfigurations.Add(factoryFactory.Invoke());

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private struct AnyParameterType { }
    }
}