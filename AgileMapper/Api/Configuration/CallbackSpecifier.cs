namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Lambdas;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class CallbackSpecifier<TSource, TTarget> : IConditionalCallbackSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly QualifiedMember _targetMember;

        public CallbackSpecifier(
            MapperContext mapperContext,
            InvocationPosition invocationPosition,
            QualifiedMember targetMember)
            : this(
                  MappingConfigInfo.AllRuleSetsAndSourceTypes(mapperContext).ForTargetType<TTarget>(),
                  invocationPosition,
                  targetMember)
        {
        }

        public CallbackSpecifier(
            MappingConfigInfo configInfo,
            InvocationPosition invocationPosition,
            QualifiedMember targetMember)
        {
            _configInfo = configInfo.WithInvocationPosition(invocationPosition);
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
            _configInfo.AddConditionOrThrow(conditionLambda);
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

            var callbackLambda = ConfiguredLambdaInfo
                .ForAction(callback, _configInfo, typeof(TSource), typeof(TTarget));

            var creationCallbackFactory = new MappingCallbackFactory(
                _configInfo,
                callbackLambda,
                _targetMember);

            _configInfo.UserConfigurations.Add(creationCallbackFactory);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
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