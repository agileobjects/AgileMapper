namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventMappingConfigStartingPoint<TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public CallbackSpecifier<object> CreatingInstances
            => new CallbackSpecifier<object>(_configInfo);

        public CallbackSpecifier<TTarget> CreatingTargetInstances
            => new CallbackSpecifier<TTarget>(_configInfo);

        public CallbackSpecifier<TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => new CallbackSpecifier<TInstance>(_configInfo, typeof(TInstance));
    }
}