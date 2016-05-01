namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;

    public class CustomDataSourceTargetMemberSpecifier<TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly Func<Expression, Expression> _customValueFactory;

        internal CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            Func<Expression, Expression> customValueFactory)
        {
            _configInfo = configInfo;
            _customValueFactory = customValueFactory;
        }

        public void To<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            RegisterDataSource<TTargetValue>(targetMember);
        }

        public void To<TTargetValue>(Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
        {
            RegisterDataSource<TTargetValue>(targetSetMethod);
        }

        private void RegisterDataSource<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            _configInfo.ThrowIfSourceTypeDoesNotMatch<TTargetValue>();

            var configuredDataSourceFactory = ConfiguredDataSourceFactory.For(
                _configInfo,
                typeof(TTarget),
                _customValueFactory,
                targetMemberLambda.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);
        }
    }
}