namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public class SourceAndTargetCallbackSpecifier<TSource, TTarget> : ObjectCallbackSpecifier<TTarget>
    {
        internal SourceAndTargetCallbackSpecifier(CallbackPosition callbackPosition, MappingConfigInfo configInfo)
            : this(callbackPosition, configInfo, typeof(TTarget))
        {
        }

        internal SourceAndTargetCallbackSpecifier(
            CallbackPosition callbackPosition,
            MappingConfigInfo configInfo,
            Type targetType)
            : base(callbackPosition, configInfo, targetType, Callbacks.Target, Callbacks.SourceAndTarget)
        {
        }

        public void Call(Action<TSource, TTarget> callback)
        {
            AddCallback(Expression.Constant(callback), typeof(TSource), typeof(TTarget));
        }
    }
}