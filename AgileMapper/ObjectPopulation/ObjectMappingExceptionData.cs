namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides factory methods for creating <see cref="IMappingExceptionData"/> instances.
    /// </summary>
    public static class ObjectMappingExceptionData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectMappingExceptionData).GetPublicStaticMethod(nameof(Create));

        internal static readonly MethodInfo CreateTypedMethod =
            typeof(ObjectMappingExceptionData).GetPublicStaticMethod(nameof(CreateTyped));

        /// <summary>
        /// Creates an <see cref="IMappingExceptionData"/> for the given arguments.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of source object that was being mapped from when the Exception occurred.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The type of target object that was being mapped to when the Exception occurred.
        /// </typeparam>
        /// <param name="mappingData">
        /// The <see cref="IMappingData{TSource, TTarget}"/> containing the data being mapped when
        /// the Exception occurred.
        /// </param>
        /// <param name="exception">The Exception which occurred.</param>
        /// <returns>
        /// An <see cref="IMappingExceptionData"/> providing information about the given, thrown
        /// <see cref="Exception"/>.
        /// </returns>
        public static IMappingExceptionData Create<TSource, TTarget>(
            IMappingData<TSource, TTarget> mappingData,
            Exception exception)
        {
            return new ObjectMappingExceptionData<TSource, TTarget>(mappingData, exception);
        }

        /// <summary>
        /// Creates an <see cref="IMappingExceptionData{TSource, TTarget}"/> for the given arguments.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of source object that was being mapped from when the Exception occurred.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The type of target object that was being mapped to when the Exception occurred.
        /// </typeparam>
        /// <param name="mappingData">
        /// The <see cref="IMappingData{TSource, TTarget}"/> containing the data being mapped when
        /// the Exception occurred.
        /// </param>
        /// <param name="exception">The Exception which occurred.</param>
        /// <returns>
        /// An <see cref="IMappingExceptionData{TSource, TTarget}"/> providing information about the
        /// given, thrown <see cref="Exception"/>.
        /// </returns>
        public static IMappingExceptionData<TSource, TTarget> CreateTyped<TSource, TTarget>(
            IMappingData<TSource, TTarget> mappingData,
            Exception exception)
        {
            return new ObjectMappingExceptionData<TSource, TTarget>(mappingData, exception);
        }
    }

    internal class ObjectMappingExceptionData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingExceptionData,
        IMappingExceptionData<TSource, TTarget>
    {
        public ObjectMappingExceptionData(IMappingData<TSource, TTarget> mappingData, Exception exception)
            : base(mappingData)
        {
            Exception = exception;
        }

        object IMappingExceptionData.Source => Source;

        object IMappingExceptionData.Target => Target;

        public Exception Exception { get; }
    }
}