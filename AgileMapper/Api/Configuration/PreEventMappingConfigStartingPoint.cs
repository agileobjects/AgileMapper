namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PreEventMappingConfigStartingPoint<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PreEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingTargetInstances
            => CreateCallbackSpecifier<TTarget>();

        public IConditionalPreInstanceCreationCallbackSpecifier<TSource, TTarget> CreatingInstancesOf<TInstance>()
            where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private PreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance> CreateCallbackSpecifier<TInstance>()
            => new PreInstanceCreationCallbackSpecifier<TSource, TTarget, TInstance>(_configInfo);
    }
}