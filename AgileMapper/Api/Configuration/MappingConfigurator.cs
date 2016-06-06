namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class MappingConfigurator<TSource, TTarget> : IFullMappingConfigurator<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public MappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        #region If Overloads

        public IRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        public IRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private IRootMappingConfigurator<TSource, TTarget> SetCondition(LambdaExpression conditionLambda)
        {
            _configInfo.AddCondition(conditionLambda);
            return this;
        }

        #endregion

        public void CreateInstancesUsing(Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TTarget>> factory)
        {
            var objectFactory = ConfiguredObjectFactory.For(_configInfo, typeof(TTarget), factory);

            _configInfo.MapperContext.UserConfigurations.Add(objectFactory);
        }

        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public void PassExceptionsTo(Action<ITypedMemberMappingExceptionContext<TSource, TTarget>> callback)
        {
            var callbackFactory = new ExceptionCallbackFactory(
                _configInfo,
                typeof(TTarget),
                Expression.Constant(callback));

            _configInfo.MapperContext.UserConfigurations.Add(callbackFactory);
        }

        public void Ignore<TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            var configuredIgnoredMember = ConfiguredIgnoredMember.For(
                _configInfo,
                typeof(TTarget),
                targetMember.Body);

            _configInfo.MapperContext.UserConfigurations.Add(configuredIgnoredMember);
            _configInfo.NegateCondition();
        }

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before => new PreEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After => new PostEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        #region Map Overloads

        public CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TTarget>(
                _configInfo.ForSourceValueType(typeof(TSourceValue)),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TTarget> MapFunc<TSourceValue>(Func<TSource, TSourceValue> valueFunc)
            => GetConstantTargetMemberSpecifier(valueFunc);

        public CustomDataSourceTargetMemberSpecifier<TTarget> Map<TSourceValue>(TSourceValue value)
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(value, typeof(TSource), typeof(TTarget));

            return (valueLambdaInfo != null)
                ? new CustomDataSourceTargetMemberSpecifier<TTarget>(
                    _configInfo.ForSourceValueType(valueLambdaInfo.ReturnType),
                    valueLambdaInfo)
                : GetConstantTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TTarget> GetConstantTargetMemberSpecifier<TSourceValue>(TSourceValue value)
        {
            var valueConstant = Expression.Constant(value, typeof(TSourceValue));
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TTarget>(
                _configInfo.ForSourceValueType(valueConstant.Type),
                valueLambda);
        }

        #endregion

        public DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget> Map<TDerivedSource>() where TDerivedSource : TSource
            => new DerivedPairTargetTypeSpecifier<TDerivedSource, TTarget>(_configInfo);

        #endregion
    }
}