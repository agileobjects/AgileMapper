namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides factory methods for creating
    /// <see cref="IObjectCreationMappingData{TSource, TTarget, TObject}"/> instances.
    /// </summary>
    public static class ObjectCreationMappingData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectCreationMappingData).GetPublicStaticMethod(nameof(Create));

        /// <summary>
        /// Creates an <see cref="IObjectCreationMappingData{TSource, TTarget, TObject}"/> for the
        /// given arguments.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of source object that was being mapped from when the object was created.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The type of target object that was being mapped to when the object was created.
        /// </typeparam>
        /// <typeparam name="TObject">The type of object which was created.</typeparam>
        /// <param name="mappingData">
        /// The <see cref="IMappingData{TSource, TTarget}"/> containing the data being mapped when
        /// the object was created.
        /// </param>
        /// <param name="createdCbject">The object which was created.</param>
        /// <returns>
        /// An <see cref="IObjectCreationMappingData{TSource, TTarget, TObject}"/> providing
        /// information about the given <paramref name="createdCbject"/>'s creation.
        /// </returns>
        public static IObjectCreationMappingData<TSource, TTarget, TObject> Create<TSource, TTarget, TObject>(
            IMappingData<TSource, TTarget> mappingData,
            TObject createdCbject)
        {
            return new ObjectCreationMappingData<TSource, TTarget, TObject>(mappingData, createdCbject);
        }
    }

    internal class ObjectCreationMappingData<TSource, TTarget, TObject> :
        MappingInstanceData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TObject>
    {
        private readonly TObject _createdObject;

        public ObjectCreationMappingData(IMappingData<TSource, TTarget> mappingData, TObject createdObject)
            : base(mappingData)
        {
            _createdObject = createdObject;
        }

        TObject IObjectCreationMappingData<TSource, TTarget, TObject>.CreatedObject => _createdObject;
    }
}