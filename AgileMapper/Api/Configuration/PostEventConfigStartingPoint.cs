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

        public ObjectCallbackSpecifier<object, object, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public ObjectCallbackSpecifier<object, object, TInstance> CreatingInstancesOf<TInstance>() where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private ObjectCallbackSpecifier<object, object, TInstance> CreateCallbackSpecifier<TInstance>()
            => new ObjectCallbackSpecifier<object, object, TInstance>(
                   CallbackPosition.After,
                   _mapperContext,
                   Callbacks.Target,
                   Callbacks.SourceAndTarget);
    }
}