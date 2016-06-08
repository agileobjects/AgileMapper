namespace AgileObjects.AgileMapper.Api.Configuration
{
    using ObjectPopulation;

    public class PreEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PreEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public IConditionalCallbackSpecifier<object, object> MappingBegins
            => new CallbackSpecifier<object, object>(CallbackPosition.Before, _mapperContext);

        public IConditionalPreInstanceCreationCallbackSpecifier<object, object, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        private InstanceCreationCallbackSpecifier<object, object, TInstance> CreateCallbackSpecifier<TInstance>()
            => new InstanceCreationCallbackSpecifier<object, object, TInstance>(CallbackPosition.Before, _mapperContext);
    }
}