namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PostEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PostEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public ObjectCallbackSpecifier<object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public ObjectCallbackSpecifier<TTarget> CreatingInstancesOf<TTarget>() where TTarget : class
            => CreateCallbackSpecifier<TTarget>();

        private ObjectCallbackSpecifier<TInstance> CreateCallbackSpecifier<TInstance>()
            => new ObjectCallbackSpecifier<TInstance>(
                   CallbackPosition.After,
                   _mapperContext,
                   Callbacks.Target,
                   Callbacks.SourceAndTarget);
    }
}