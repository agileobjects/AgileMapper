namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;

    internal static class ObjectMappingExceptionData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectMappingExceptionData).GetPublicStaticMethod("Create");

        public static ObjectMappingExceptionData<TSource, TTarget> Create<TSource, TTarget>(
            IMappingData<TSource, TTarget> mappingData,
            Exception exception)
            => new ObjectMappingExceptionData<TSource, TTarget>(mappingData, exception);
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