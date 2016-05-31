namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    public class MappingConfigurator<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal MappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public void CreateInstancesUsing(Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TTarget>> factory)
        {
            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TTarget), factory);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }

        public void PassExceptionsTo(Action<ITypedMemberMappingExceptionContext<TSource, TTarget>> callback)
        {
            var callbackFactory = new ExceptionCallbackFactory(
                _configInfo,
                typeof(TTarget),
                Expression.Constant(callback));

            _configInfo.MapperContext.UserConfigurations.Add(callbackFactory);
        }

        public ConditionSpecifier<TSource, TTarget> Ignore<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            var configuredIgnoredMember = ConfiguredIgnoredMember.For(
                _configInfo,
                typeof(TTarget),
                targetMember.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredIgnoredMember);

            return new ConditionSpecifier<TSource, TTarget>(configuredIgnoredMember, negateCondition: true);
        }

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before => new PreEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After => new PostEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        #region Map Overloads

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(Func<TSource, TSourceValue> valueFunc)
            => GetConstantTargetMemberSpecifier(valueFunc);

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(value, typeof(TSource), typeof(TTarget));

            return (valueLambdaInfo != null)
                ? new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                    _configInfo.ForSourceValueType(valueLambdaInfo.ReturnType),
                    valueLambdaInfo)
                : GetConstantTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantTargetMemberSpecifier<TSourceValue>(TSourceValue value)
        {
            var valueConstant = Expression.Constant(value, typeof(TSourceValue));
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(valueConstant.Type),
                valueLambda);
        }

        #endregion

        public DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget> Map<TDerivedSource>() where TDerivedSource : TSource
            => new DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget>(_configInfo);

        #endregion
    }
}