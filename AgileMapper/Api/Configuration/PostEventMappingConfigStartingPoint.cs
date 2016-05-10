namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PostEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public SourceAndTargetCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public SourceAndTargetCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public SourceAndTargetCallbackSpecifier<TSource, TTarget, TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private SourceAndTargetCallbackSpecifier<TSource, TTarget, TInstance> CreateCallbackSpecifier<TInstance>()
            => new SourceAndTargetCallbackSpecifier<TSource, TTarget, TInstance>(CallbackPosition.After, _configInfo);
    }
}