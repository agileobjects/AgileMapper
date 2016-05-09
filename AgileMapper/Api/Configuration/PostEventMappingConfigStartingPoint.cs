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

        public SourceAndTargetCallbackSpecifier<TSource, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public SourceAndTargetCallbackSpecifier<TSource, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public SourceAndTargetCallbackSpecifier<TSource, TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private SourceAndTargetCallbackSpecifier<TSource, TInstance> CreateCallbackSpecifier<TInstance>()
            => new SourceAndTargetCallbackSpecifier<TSource, TInstance>(CallbackPosition.After, _configInfo, typeof(TInstance));
    }
}