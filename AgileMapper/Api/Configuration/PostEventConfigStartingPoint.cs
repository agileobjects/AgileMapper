namespace AgileObjects.AgileMapper.Api.Configuration
{
    using Members;
    using ObjectPopulation;

    public class PostEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PostEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public IConditionalCallbackSpecifier<object, object> MappingEnds
            => new CallbackSpecifier<object, object>(_mapperContext, CallbackPosition.After, QualifiedMember.None);

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, object> CreatingInstances
            => CreatingInstancesOf<object>();

        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, TObject> CreatingInstancesOf<TObject>()
            where TObject : class
            => new InstanceCreationCallbackSpecifier<object, object, TObject>(CallbackPosition.After, _mapperContext);
    }
}