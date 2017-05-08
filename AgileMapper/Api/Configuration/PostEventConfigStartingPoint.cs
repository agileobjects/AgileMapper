namespace AgileObjects.AgileMapper.Api.Configuration
{
    using Members;
    using ObjectPopulation;

    /// <summary>
    /// Provides options for configuring the execution of a callback after a particular type of event for all
    /// source and target types.
    /// </summary>
    public class PostEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PostEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        /// <summary>
        /// Configure a callback to be executed after any object mapping ends.
        /// </summary>
        public IConditionalCallbackSpecifier<object, object> MappingEnds
            => new CallbackSpecifier<object, object>(_mapperContext, CallbackPosition.After, QualifiedMember.None);

        /// <summary>
        /// Configure a callback to be executed after instances of any object are created during any object 
        /// mapping.
        /// </summary>
        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, object> CreatingInstances
            => CreatingInstancesOf<object>();

        /// <summary>
        /// Configure a callback to be executed after instances of the given type argument are created during 
        /// any object mapping.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object the creation of which the callback execution should follow.
        /// </typeparam>
        public IConditionalPostInstanceCreationCallbackSpecifier<object, object, TObject> CreatingInstancesOf<TObject>()
            => new InstanceCreationCallbackSpecifier<object, object, TObject>(CallbackPosition.After, _mapperContext);
    }
}