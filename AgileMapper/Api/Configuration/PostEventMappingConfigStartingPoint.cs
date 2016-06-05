namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreatingInstancesOf<TInstance>()
            where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreateCallbackSpecifier<TInstance>()
            => new PostInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>(_configInfo);
    }
}