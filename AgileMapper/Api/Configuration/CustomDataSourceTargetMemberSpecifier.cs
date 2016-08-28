namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using DataSources;
    using Members;
    using ReadableExpressions.Extensions;

    public class CustomDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly ConfiguredLambdaInfo _customValueLambda;

        internal CustomDataSourceTargetMemberSpecifier(MappingConfigInfo configInfo, LambdaExpression customValueLambda)
            : this(configInfo, ConfiguredLambdaInfo.For(customValueLambda))
        {
        }

        internal CustomDataSourceTargetMemberSpecifier(MappingConfigInfo configInfo, ConfiguredLambdaInfo customValueLambda)
        {
            _configInfo = configInfo;
            _customValueLambda = customValueLambda;
        }

        public MappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterDataSource<TTargetValue>(() => CreateFromLambda(targetMember));

        public MappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterDataSource<TTargetValue>(() => CreateFromLambda(targetSetMethod));

        private ConfiguredDataSourceFactory CreateFromLambda(LambdaExpression targetMemberLambda)
            => new ConfiguredDataSourceFactory(_configInfo, _customValueLambda, targetMemberLambda);

        public MappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        public MappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName)
            => RegisterDataSource<object>(() => CreateForCtorParam(parameterName));

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>()
            => CreateForCtorParam(GetUniqueConstructorParameterOrThrow<TParam>());

        private ConfiguredDataSourceFactory CreateForCtorParam(string name)
            => CreateForCtorParam(GetUniqueConstructorParameterOrThrow<AnyParameterType>(name));

        private static ParameterInfo GetUniqueConstructorParameterOrThrow<TParam>(string name = null)
        {
            var ignoreParameterType = typeof(TParam) == typeof(AnyParameterType);
            var ignoreParameterName = name == null;

            var matchingParameters = typeof(TTarget)
                .GetConstructors(Constants.PublicInstance)
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
                ThrowMissingParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameterData = matchingParameters.First();

            if (matchingParameterData.MatchingParameters.Length > 1)
            {
                ThrowAmbiguousParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameter = matchingParameterData.MatchingParameters.First();

            return matchingParameter;
        }

        private static string GetParameterMatchInfo<TParam>(string name, bool matchParameterType)
            => matchParameterType ? "of type " + typeof(TParam).GetFriendlyName() : "named '" + name + "'";

        private static void ThrowMissingParameterException(string parameterMatchInfo)
        {
            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "No constructor parameter {0} exists on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private static void ThrowAmbiguousParameterException(string parameterMatchInfo)
        {
            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Multiple constructor parameters found {0} on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private ConfiguredDataSourceFactory CreateForCtorParam(ParameterInfo parameter)
        {
            var memberChain = new[]
            {
                Member.RootTarget<TTarget>(),
                Member.ConstructorParameter(parameter)
            };

            var constructorParameter = QualifiedMember.From(memberChain, _configInfo.MapperContext);

            return new ConfiguredDataSourceFactory(_configInfo, _customValueLambda, constructorParameter);
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            Func<ConfiguredDataSourceFactory> factoryFactory)
        {
            _configInfo.ThrowIfSourceTypeUnconvertible<TTargetValue>();

            _configInfo.ForTargetType<TTarget>();
            var configuredDataSourceFactory = factoryFactory.Invoke();

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private struct AnyParameterType { }
    }
}