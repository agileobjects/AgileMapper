namespace AgileObjects.AgileMapper.Members
{
    using System;

    /// <summary>
    /// Provides the data being used at a particular point during a mapping when an
    /// Exception occurred.
    /// </summary>
    public interface IMappingExceptionData : IServiceProviderAccessor
    {
        /// <summary>
        /// Gets the source object that was being being mapped from when the mapping
        /// Exception occurred.
        /// </summary>
        object Source { get; }

        /// <summary>
        /// Gets the target object that was being being mapped to when the mapping
        /// Exception occurred.
        /// </summary>
        object Target { get; }

        /// <summary>
        /// Gets the index of the current enumerable being mapped when the mapping
        /// Exception occurred, if applicable.
        /// </summary>
        int? EnumerableIndex { get; }

        /// <summary>
        /// Get the Exception object describing the error that occurred during the mapping.
        /// </summary>
        Exception Exception { get; }
    }

    /// <summary>
    /// Provides typed data being used at a particular point during a mapping when an
    /// Exception occurred.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object that was being mapped from when the Exception occurred.
    /// </typeparam>
    /// <typeparam name="TTarget">
    /// The type of target object that was being mapped to when the Exception occurred.
    /// </typeparam>
    public interface IMappingExceptionData<out TSource, out TTarget>
        : IMappingData<TSource, TTarget>
    {
        /// <summary>
        /// Get the Exception object describing the error that occurred during the mapping.
        /// </summary>
        Exception Exception { get; }
    }
}