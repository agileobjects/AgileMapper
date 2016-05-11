namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public class InstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> : CallbackSpecifierBase
    {
        internal InstanceCreationCallbackSpecifier(CallbackPosition callbackPosition, MapperContext mapperContext)
            : this(
                callbackPosition,
                new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes())
        {
        }

        internal InstanceCreationCallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(callbackPosition, configInfo)
        {
        }

        public InstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<IInstanceCreationContext<TSource, TTarget, TInstance>> callback) => CreateCallback(callback);

        public InstanceCreationConditionSpecifier<TSource, TTarget, TInstance> Call(
            Action<TSource, TTarget, TInstance> callback) => CreateCallback(callback);

        private InstanceCreationConditionSpecifier<TSource, TTarget, TInstance> CreateCallback<TAction>(TAction callback)
        {
            var callbackLambda = ConfiguredLambdaInfo.ForAction(callback, typeof(TSource), typeof(TTarget), typeof(TInstance));

            var creationCallbackFactory = new ObjectCreationCallbackFactory(
                ConfigInfo,
                typeof(TTarget),
                typeof(TInstance),
                CallbackPosition,
                callbackLambda);

            ConfigInfo.MapperContext.UserConfigurations.Add(creationCallbackFactory);

            return new InstanceCreationConditionSpecifier<TSource, TTarget, TInstance>(creationCallbackFactory);
        }
    }
}