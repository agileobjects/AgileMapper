namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

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
            => new FactorySpecifier<TSource, TTarget, TTarget>(_configInfo).Using(factory);

        public void CreateInstancesUsing<TFactory>(TFactory factory) where TFactory : class
            => new FactorySpecifier<TSource, TTarget, TTarget>(_configInfo).Using(factory);

        public IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() where TObject : class
            => new FactorySpecifier<TSource, TTarget, TObject>(_configInfo);

        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public void PassExceptionsTo(Action<ITypedMemberMappingExceptionContext<TSource, TTarget>> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                _configInfo.ForTargetType<TTarget>(),
                Expression.Constant(callback));

            _configInfo.MapperContext.UserConfigurations.Add(exceptionCallback);
        }

        public MappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers)
        {
            var configInfo = _configInfo.ForTargetType<TTarget>();

            foreach (var targetMember in targetMembers)
            {
                var configuredIgnoredMember = new ConfiguredIgnoredMember(
                configInfo,
                targetMember);

                _configInfo.MapperContext.UserConfigurations.Add(configuredIgnoredMember);
                _configInfo.NegateCondition();
            }

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        public PreEventMappingConfigStartingPoint<TSource, TTarget> Before
            => new PreEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        public PostEventMappingConfigStartingPoint<TSource, TTarget> After
            => new PostEventMappingConfigStartingPoint<TSource, TTarget>(_configInfo);

        #region Map Overloads

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<ITypedMemberMappingContext<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(
            Expression<Func<TSource, TTarget, int?, TSourceValue>> valueFactoryExpression)
        {
            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType<TSourceValue>(),
                valueFactoryExpression);
        }

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> MapFunc<TSourceValue>(
            Func<TSource, TSourceValue> valueFunc)
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

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantTargetMemberSpecifier<TSourceValue>(
            TSourceValue value)
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