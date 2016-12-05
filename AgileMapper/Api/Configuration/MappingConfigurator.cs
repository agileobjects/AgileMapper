namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;

    internal class MappingConfigurator<TSource, TTarget> :
        IFullMappingConfigurator<TSource, TTarget>,
        IConditionalRootMappingConfigurator<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public MappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        #region If Overloads

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public IConditionalRootMappingConfigurator<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private IConditionalRootMappingConfigurator<TSource, TTarget> SetCondition(LambdaExpression conditionLambda)
        {
            _configInfo.AddConditionOrThrow(conditionLambda);
            return this;
        }

        #endregion

        public MappingConfigContinuation<TSource, TTarget> CreateInstancesUsing(Expression<Func<IMappingData<TSource, TTarget>, TTarget>> factory)
        {
            new FactorySpecifier<TSource, TTarget, TTarget>(_configInfo).Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        public MappingConfigContinuation<TSource, TTarget> CreateInstancesUsing<TFactory>(TFactory factory) where TFactory : class
        {
            new FactorySpecifier<TSource, TTarget, TTarget>(_configInfo).Using(factory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        public IFactorySpecifier<TSource, TTarget, TObject> CreateInstancesOf<TObject>() where TObject : class
            => new FactorySpecifier<TSource, TTarget, TObject>(_configInfo);

        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public void PassExceptionsTo(Action<IMappingExceptionData<TSource, TTarget>> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                _configInfo.ForTargetType<TTarget>(),
                Expression.Constant(callback));

            _configInfo.MapperContext.UserConfigurations.Add(exceptionCallback);
        }

        public MappingConfigContinuation<TSource, TTarget> TrackMappedObjects()
        {
            var trackingMode = new ObjectTrackingMode(_configInfo.ForTargetType<TTarget>());

            _configInfo.MapperContext.UserConfigurations.Add(trackingMode);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        public void MapNullCollectionsToNull()
        {
            var nullSetting = new NullCollectionsSetting(_configInfo.ForTargetType<TTarget>());

            _configInfo.MapperContext.UserConfigurations.Add(nullSetting);
        }

        public MappingConfigContinuation<TSource, TTarget> Ignore(params Expression<Func<TTarget, object>>[] targetMembers)
        {
            var configInfo = _configInfo.ForTargetType<TTarget>();

            foreach (var targetMember in targetMembers)
            {
                var configuredIgnoredMember =
                    new ConfiguredIgnoredMember(configInfo, targetMember);

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
            Expression<Func<IMappingData<TSource, TTarget>, TSourceValue>> valueFactoryExpression)
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
            => GetConstantValueTargetMemberSpecifier(valueFunc);

        public CustomDataSourceTargetMemberSpecifier<TSource, TTarget> Map<TSourceValue>(TSourceValue value)
        {
            var valueLambdaInfo = ConfiguredLambdaInfo.ForFunc(value, typeof(TSource), typeof(TTarget));

            return (valueLambdaInfo != null)
                ? new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                    _configInfo.ForSourceValueType(valueLambdaInfo.ReturnType),
                    valueLambdaInfo)
                : GetConstantValueTargetMemberSpecifier(value);
        }

        #region Map Helpers

        private CustomDataSourceTargetMemberSpecifier<TSource, TTarget> GetConstantValueTargetMemberSpecifier<TSourceValue>(
            TSourceValue value)
        {
            var valueConstant = Expression.Constant(value, typeof(TSourceValue));
            var valueLambda = Expression.Lambda<Func<TSourceValue>>(valueConstant);

            return new CustomDataSourceTargetMemberSpecifier<TSource, TTarget>(
                _configInfo.ForSourceValueType(valueConstant.Type),
                valueLambda);
        }

        #endregion

        public MappingConfigContinuation<TSource, TTarget> MapTo<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypePair = new DerivedPairTargetTypeSpecifier<TSource, TSource, TTarget>(_configInfo);

            return derivedTypePair.To<TDerivedTarget>();
        }

        public DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget> Map<TDerivedSource>() where TDerivedSource : TSource
            => new DerivedPairTargetTypeSpecifier<TSource, TDerivedSource, TTarget>(_configInfo);

        #endregion
    }
}