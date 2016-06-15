namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;

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
            => RegisterDataSource<TTargetValue>(targetMember);

        public MappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterDataSource<TTargetValue>(targetSetMethod);

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            LambdaExpression targetMemberLambda)
        {
            _configInfo.ThrowIfSourceTypeDoesNotMatch<TTargetValue>();

            var configuredDataSourceFactory = new ConfiguredDataSourceFactory(
                _configInfo.ForTargetType<TTarget>(),
                _customValueLambda,
                targetMemberLambda);

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }
    }
}