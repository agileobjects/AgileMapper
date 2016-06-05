namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    internal abstract class InstanceCreationCallbackSpecifierBase<TSource, TTarget, TInstance> : CallbackSpecifierBase
    {
        protected InstanceCreationCallbackSpecifierBase(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(callbackPosition, configInfo)
        {
        }

        protected ObjectCreationCallbackFactory CreateCallbackFactory<TAction>(TAction callback)
        {
            var callbackLambda = ConfiguredLambdaInfo.ForAction(callback, typeof(TSource), typeof(TTarget), typeof(TInstance));

            var creationCallbackFactory = new ObjectCreationCallbackFactory(
                ConfigInfo,
                typeof(TTarget),
                typeof(TInstance),
                CallbackPosition,
                callbackLambda);

            ConfigInfo.MapperContext.UserConfigurations.Add(creationCallbackFactory);

            return creationCallbackFactory;
        }
    }
}