namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using DataSources;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides options for specifying a target member to which a configuration option should apply.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class CustomDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _customValueLambda;
        private readonly ConfiguredLambdaInfo _customValueLambdaInfo;

        internal CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            LambdaExpression customValueLambda)
            : this(configInfo, default(ConfiguredLambdaInfo))
        {
            _customValueLambda = customValueLambda;
        }

        internal CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo customValueLambda)
        {
            _configInfo = configInfo;
            _customValueLambdaInfo = customValueLambda;
        }

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        public MappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetMember));

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetSetMethod"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The type of the target set method's argument.</typeparam>
        /// <param name="targetSetMethod">The target set method to which to apply the configuration.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        public MappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
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

            var rootMember = (DictionaryTargetMember)_configInfo.MapperContext
                .QualifiedMemberFactory
                .RootTarget<TSource, TTarget>();

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
                 typeof(TTargetValue).IsAssignableFrom(_customValueLambda.ReturnType))
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

        /// <summary>
        /// Apply the configuration to the constructor parameter with the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTargetParam">The target constructor parameter's type.</typeparam>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        public MappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        /// <summary>
        /// Apply the configuration to the constructor parameter with the specified <paramref name="parameterName"/>.
        /// </summary>
        /// <param name="parameterName">The target constructor parameter's name.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        public MappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName)
            => RegisterDataSource<object>(() => CreateForCtorParam(parameterName));

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