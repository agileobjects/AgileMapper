namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    public class PreEventMappingConfigStartingPoint<TSource, TTarget> : MappingConfigStartingPointBase<TSource, TTarget>
    {
        internal PreEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
            : base(configInfo, CallbackPosition.Before)
        {
        }

        public IConditionalCallbackSpecifier<TSource, TTarget> MappingBegins => CreateCallbackSpecifier();

        public IConditionalCallbackSpecifier<TSource, TTarget> Mapping<TMember>(Expression<Func<TTarget, TMember>> targetMember)
            => CreateCallbackSpecifier(targetMember);

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreatingInstancesOf<TInstance>()
            where TInstance : class
            => CreateCallbackSpecifier<TInstance>();
    }
}