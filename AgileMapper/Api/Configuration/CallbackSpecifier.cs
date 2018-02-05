namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class CallbackSpecifier<TSource, TTarget> :
        CallbackSpecifierBase,
        IConditionalCallbackSpecifier<TSource, TTarget>
    {
        private readonly QualifiedMember _targetMember;

        public CallbackSpecifier(
            MapperContext mapperContext,
            CallbackPosition callbackPosition,
            QualifiedMember targetMember)
            : this(
                  MappingConfigInfo.AllRuleSetsAndSourceTypes(mapperContext).ForTargetType<TTarget>(),
                  callbackPosition,
                  targetMember)
        {
        }

        public CallbackSpecifier(
            MappingConfigInfo configInfo,
            CallbackPosition callbackPosition,
            QualifiedMember targetMember)
            : base(callbackPosition, configInfo)
        {
            _targetMember = targetMember;
        }

        public ICallbackSpecifier<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition)
            => SetCondition(condition);

        public ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition)
            => SetCondition(condition);

        public ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition)
            => SetCondition(condition);

        private CallbackSpecifier<TSource, TTarget> SetCondition(LambdaExpression conditionLambda)
        {
            ConfigInfo.AddConditionOrThrow(conditionLambda);
            return this;
        }

        public IMappingConfigContinuation<TSource, TTarget> Call(Action<IMappingData<TSource, TTarget>> callback)
            => CreateCallbackFactory(callback);

        public IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback)
            => CreateCallbackFactory(callback);

        public IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, int?> callback)
            => CreateCallbackFactory(callback);

        private MappingConfigContinuation<TSource, TTarget> CreateCallbackFactory<TAction>(TAction callback)
        {
            ThrowIfStructMemberCallback();

            var callbackLambda = ConfiguredLambdaInfo.ForAction(callback, typeof(TSource), typeof(TTarget));

            var creationCallbackFactory = new MappingCallbackFactory(
                ConfigInfo,
                CallbackPosition,
                callbackLambda,
                _targetMember);

            ConfigInfo.MapperContext.UserConfigurations.Add(creationCallbackFactory);

            return new MappingConfigContinuation<TSource, TTarget>(ConfigInfo);
        }

        private void ThrowIfStructMemberCallback()
        {
            if ((_targetMember == QualifiedMember.All) || typeof(TTarget).IsClass())
            {
                return;
            }

            throw new MappingConfigurationException(
                "Cannot configure struct member population callbacks",
                new NotSupportedException(
                    "Structs are populated with Member Initialisations, so cannot have member population callbacks"));
        }
    }
}