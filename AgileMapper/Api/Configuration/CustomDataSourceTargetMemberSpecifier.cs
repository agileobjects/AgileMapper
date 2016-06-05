namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;

    public class CustomDataSourceTargetMemberSpecifier<TTarget>
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

        public void To<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterDataSource<TTargetValue>(targetMember);

        public void To<TTargetValue>(Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterDataSource<TTargetValue>(targetSetMethod);

        private void RegisterDataSource<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            _configInfo.ThrowIfSourceTypeDoesNotMatch<TTargetValue>();

            var configuredDataSourceFactory = ConfiguredDataSourceFactory.For(
                _configInfo,
                _customValueLambda,
                typeof(TTarget),
                targetMemberLambda.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);
        }
    }
}