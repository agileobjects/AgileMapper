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
                ThrowMissingParameterException<TParam>(name, !ignoreParameterType, !ignoreParameterName);
            }

            var matchingParameterData = matchingParameters.First();
            var matchingParameter = matchingParameterData.MatchingParameters.First();

            return matchingParameter;
        }

        private static void ThrowMissingParameterException<TParam>(string name, bool matchParameterType, bool matchParameterName)
        {
            var parameterMatchInfo = "";

            if (matchParameterType)
            {
                parameterMatchInfo = "of type " + typeof(TParam).GetFriendlyName();
            }

            if (matchParameterName)
            {
                if (matchParameterType)
                {
                    parameterMatchInfo += " ";
                }

                parameterMatchInfo += "named '" + name + "'";
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "No constructor parameter {0} exists on type {1}",
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