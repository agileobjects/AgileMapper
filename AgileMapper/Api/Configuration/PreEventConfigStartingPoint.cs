namespace AgileObjects.AgileMapper.Api.Configuration
{
    using Members;
    using ObjectPopulation;

    /// <summary>
    /// Provides options for configuring the execution of a callback before a particular type of event for all
    /// source and target types.
    /// </summary>
    public class PreEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PreEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        /// <summary>
        /// Configure a callback to be executed before any object mapping begins.
        /// </summary>
        public IConditionalCallbackSpecifier<object, object> MappingBegins
            => new CallbackSpecifier<object, object>(_mapperContext, CallbackPosition.Before, QualifiedMember.None);

        /// <summary>
        /// Configure a callback to be executed before instances of any object are created during any object 
        /// mapping.
        /// </summary>
        public IConditionalPreInstanceCreationCallbackSpecifier<object, object> CreatingInstances
            => CreatingInstancesOf<object>();

        /// <summary>
        /// Configure a callback to be executed before instances of the given type argument are created during 
        /// any object mapping.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object the creation of which the callback execution should precede.
        /// </typeparam>
        public IConditionalPreInstanceCreationCallbackSpecifier<object, object> CreatingInstancesOf<TObject>()
            where TObject : class
            => new InstanceCreationCallbackSpecifier<object, object, TObject>(CallbackPosition.Before, _mapperContext);
    }
}