namespace AgileObjects.AgileMapper.Api.Configuration
{
    using Members;
    using ObjectPopulation;

    public class PreEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PreEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public IConditionalCallbackSpecifier<object, object> MappingBegins
            => new CallbackSpecifier<object, object>(_mapperContext, CallbackPosition.Before, QualifiedMember.None);

        public IConditionalPreInstanceCreationCallbackSpecifier<object, object> CreatingInstances
            => CreatingInstancesOf<object>();

        public IConditionalPreInstanceCreationCallbackSpecifier<object, object> CreatingInstancesOf<TObject>()
            where TObject : class
            => new InstanceCreationCallbackSpecifier<object, object, TObject>(CallbackPosition.Before, _mapperContext);
    }
}