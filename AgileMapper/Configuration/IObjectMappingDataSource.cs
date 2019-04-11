namespace AgileObjects.AgileMapper.Configuration
{
    using ObjectPopulation;

    /// <summary>
    /// Implementing classes are convertible to an <see cref="IObjectMappingDataUntyped"/> instance.
    /// This interface supports configuration and is not intended to be used from your code.
    /// </summary>
    public interface IObjectMappingDataSource
    {
        /// <summary>
        /// Converts the <see cref="IObjectMappingDataSource"/> to an <see cref="IObjectMappingDataUntyped"/>
        /// instance using the given Types.
        /// </summary>
        /// <typeparam name="TSource">The source Type to use.</typeparam>
        /// <typeparam name="TTarget">The target Type to use.</typeparam>
        /// <returns>An <see cref="IObjectMappingDataUntyped"/> instance.</returns>
        IObjectMappingDataUntyped ToMappingData<TSource, TTarget>();
    }
}