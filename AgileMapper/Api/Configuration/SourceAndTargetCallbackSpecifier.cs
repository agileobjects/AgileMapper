namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    public class SourceAndTargetCallbackSpecifier<TSource, TTarget, TInstance> : ObjectCallbackSpecifier<TSource, TTarget, TInstance>
    {
        internal SourceAndTargetCallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : base(
                  callbackPosition,
                  configInfo,
                  typeof(TInstance),
                  Callbacks.Target,
                  Callbacks.SourceAndTarget)
        {
        }

        public void Call(Action<TSource, TInstance> callback)
        {
            AddCallback(callback);
        }
    }
}