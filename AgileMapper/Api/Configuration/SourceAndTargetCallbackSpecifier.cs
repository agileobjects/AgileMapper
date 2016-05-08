namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;

    public class SourceAndTargetCallbackSpecifier<TSource, TTarget> : TargetCallbackSpecifier<TTarget>
    {
        internal SourceAndTargetCallbackSpecifier(MappingConfigInfo configInfo)
            : this(configInfo, typeof(TTarget))
        {
        }

        internal SourceAndTargetCallbackSpecifier(MappingConfigInfo configInfo, Type targetType)
            : base(configInfo, targetType)
        {
        }

        public void Call(Action<TSource, TTarget> callback)
        {
            AddCallback(
                Expression.Constant(callback),
                context => new[] { context.SourceObject, context.TargetVariable },
                typeof(TSource),
                typeof(TTarget));
        }
    }
}