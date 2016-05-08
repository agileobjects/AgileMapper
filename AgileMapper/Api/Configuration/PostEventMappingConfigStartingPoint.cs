namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventMappingConfigStartingPoint<TTarget> : PostEventConfigStartingPoint
    {
        private readonly MappingConfigInfo _configInfo;

        internal PostEventMappingConfigStartingPoint(MappingConfigInfo configInfo)
            : base(configInfo.MapperContext)
        {
            _configInfo = configInfo;
        }

        public CallbackSpecifier<TTarget> CreatingTargetInstances
            => new CallbackSpecifier<TTarget>(_configInfo);
    }
}