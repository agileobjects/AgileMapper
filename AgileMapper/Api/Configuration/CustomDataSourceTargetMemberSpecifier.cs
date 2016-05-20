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

        public ConditionSpecifier<TSource, TTarget> To<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterDataSource<TTargetValue>(targetMember);

        public ConditionSpecifier<TSource, TTarget> To<TTargetValue>(Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterDataSource<TTargetValue>(targetSetMethod);

        private ConditionSpecifier<TSource, TTarget> RegisterDataSource<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            _configInfo.ThrowIfSourceTypeDoesNotMatch<TTargetValue>();

            var configuredDataSourceFactory = ConfiguredDataSourceFactory.For(
                _configInfo,
                _customValueLambda,
                typeof(TTarget),
                targetMemberLambda.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);

            return new ConditionSpecifier<TSource, TTarget>(configuredDataSourceFactory);
        }
    }
}