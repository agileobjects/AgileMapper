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
            Action<IInstanceCreationContext<TSource, TTarget, TInstance>> callback)
        {
            var callbackLambda = CreateCallbackLambda(callback);

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