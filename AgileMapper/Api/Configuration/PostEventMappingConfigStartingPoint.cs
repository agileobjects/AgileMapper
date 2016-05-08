namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public SourceAndTargetCallbackSpecifier<TSource, object> CreatingInstances
            => new SourceAndTargetCallbackSpecifier<TSource, object>(_configInfo);

        public SourceAndTargetCallbackSpecifier<TSource, TTarget> CreatingTargetInstances
            => new SourceAndTargetCallbackSpecifier<TSource, TTarget>(_configInfo);

        public SourceAndTargetCallbackSpecifier<TSource, TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => new SourceAndTargetCallbackSpecifier<TSource, TInstance>(_configInfo, typeof(TInstance));
    }
}