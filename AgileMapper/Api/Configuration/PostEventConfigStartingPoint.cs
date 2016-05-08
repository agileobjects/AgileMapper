namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PostEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public TargetCallbackSpecifier<object> CreatingInstances
            => new TargetCallbackSpecifier<object>(_mapperContext);

        public TargetCallbackSpecifier<TTarget> CreatingInstancesOf<TTarget>() where TTarget : class
            => new TargetCallbackSpecifier<TTarget>(_mapperContext);
    }
}