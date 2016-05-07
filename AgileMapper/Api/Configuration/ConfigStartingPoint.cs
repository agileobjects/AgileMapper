namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class ConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal ConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public CallbackSpecifier<object> CreatingInstances
            => new CallbackSpecifier<object>(_mapperContext);

        public CallbackSpecifier<TTarget> CreatingInstancesOf<TTarget>() where TTarget : class
            => new CallbackSpecifier<TTarget>(_mapperContext);

        public MappingConfigStartingPoint Mapping => new MappingConfigStartingPoint(_mapperContext);
    }
}