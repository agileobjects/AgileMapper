namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using ObjectPopulation;

    public class PostEventMappingConfigStartingPoint<TSource, TTarget> : MappingConfigStartingPointBase<TSource, TTarget>
    {
        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
            : base(configInfo, CallbackPosition.After)
        {
        }

        public IConditionalCallbackSpecifier<TSource, TTarget> MappingEnds => CreateCallbackSpecifier();

        public IConditionalCallbackSpecifier<TSource, TTarget> Mapping<TMember>(Expression<Func<TTarget, TMember>> targetMember)
            => CreateCallbackSpecifier(targetMember);

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> CreatingInstancesOf<TObject>()
            where TObject : class
            => CreateCallbackSpecifier<TObject>();
    }
}