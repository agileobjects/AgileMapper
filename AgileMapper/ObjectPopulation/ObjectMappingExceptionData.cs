namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Reflection;
    using Members;

    internal static class ObjectMappingExceptionData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectMappingExceptionData).GetMethod("Create", Constants.PublicStatic);
            
        public static ObjectMappingExceptionData<TSource, TTarget> Create<TSource, TTarget>(
            IMappingData<TSource, TTarget> data,
            Exception exception)
            => new ObjectMappingExceptionData<TSource, TTarget>(data, exception);
    }

    internal class ObjectMappingExceptionData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingExceptionData,
        IMappingExceptionData<TSource, TTarget>
    {
        public ObjectMappingExceptionData(IMappingData<TSource, TTarget> data, Exception exception)
            : base(data)
        {
            Exception = exception;
        }

        object IMappingExceptionData.Source => Source;

        object IMappingExceptionData.Target => Target;

        public Exception Exception { get; }
    }
}