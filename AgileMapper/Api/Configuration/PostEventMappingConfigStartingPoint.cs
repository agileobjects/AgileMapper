namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public PostInstanceCreationCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public PostInstanceCreationCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreateCallbackSpecifier<TInstance>()
            => new PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>(_configInfo);
    }
}