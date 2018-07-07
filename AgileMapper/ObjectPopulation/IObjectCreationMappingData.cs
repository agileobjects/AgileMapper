namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    /// <summary>
    /// Provides the data being used when an object was created during a mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of source object being mapped from during the object creation.</typeparam>
    /// <typeparam name="TTarget">The type of target object being mapped to during the object creation.</typeparam>
    /// <typeparam name="TObject">The type of object that was created.</typeparam>
    public interface IObjectCreationMappingData<out TSource, out TTarget, out TObject>
        : IMappingData<TSource, TTarget>
    {
        /// <summary>
        /// Gets the object that was created.
        /// </summary>
        TObject CreatedObject { get; }
    }
}