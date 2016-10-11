namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class ObjectMappingExceptionData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(ObjectMappingExceptionData).GetPublicStaticMethod("Create");

        public static ObjectMappingExceptionData<TSource, TTarget> Create<TSource, TTarget>(
            IMappingData<TSource, TTarget> data,
            Exception exception)
            => new ObjectMappingExceptionData<TSource, TTarget>(data, exception);
    }

    internal class ObjectMappingExceptionData<TSource, TTarget> :
        MappingInstanceDataBase<TSource, TTarget>,
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