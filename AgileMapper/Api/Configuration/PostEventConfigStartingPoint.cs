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

        public IConditionalCallbackSpecifier<object, object> MappingEnds
            => new CallbackSpecifier<object, object>(CallbackPosition.After, _mapperContext);

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, TInstance> CreatingInstancesOf<TInstance>()
            where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private InstanceCreationCallbackSpecifier<object, object, TInstance> CreateCallbackSpecifier<TInstance>()
            => new InstanceCreationCallbackSpecifier<object, object, TInstance>(CallbackPosition.After, _mapperContext);
    }
}