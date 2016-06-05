namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PostEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PostEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, object> CreatingInstances
            => CreateCallbackSpecifier<object>();

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, TInstance> CreatingInstancesOf<TInstance>()
            where TInstance : class
            => CreateCallbackSpecifier<TInstance>();

        private PostInstanceCreationCallbackSpecifier<object, object, TInstance> CreateCallbackSpecifier<TInstance>()
            => new PostInstanceCreationCallbackSpecifier<object, object, TInstance>(_mapperContext);
    }
}