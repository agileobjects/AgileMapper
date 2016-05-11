namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    public class CustomDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _dataSourceLambda;
        private readonly Func<LambdaExpression, IMemberMappingContext, Expression> _customValueFactory;

        internal CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            LambdaExpression dataSourceLambda,
            Func<LambdaExpression, IMemberMappingContext, Expression> customValueFactory)
        {
            _configInfo = configInfo;
            _dataSourceLambda = dataSourceLambda;
            _customValueFactory = customValueFactory;
        }

        public ConditionSpecifier<TSource, TTarget> To<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            return RegisterDataSource<TTargetValue>(targetMember);
        }

        public ConditionSpecifier<TSource, TTarget> To<TTargetValue>(Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
        {
            return RegisterDataSource<TTargetValue>(targetSetMethod);
        }

        private ConditionSpecifier<TSource, TTarget> RegisterDataSource<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            _configInfo.ThrowIfSourceTypeDoesNotMatch<TTargetValue>();

            var configuredDataSourceFactory = ConfiguredDataSourceFactory.For(
                _configInfo,
                _dataSourceLambda,
                typeof(TTarget),
                _customValueFactory,
                targetMemberLambda.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredDataSourceFactory);

            return new ConditionSpecifier<TSource, TTarget>(configuredDataSourceFactory);
        }
    }
}