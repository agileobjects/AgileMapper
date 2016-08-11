namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Reflection;

    internal static class MappingExceptionContextData
    {
        internal static readonly MethodInfo CreateMethod =
            typeof(MappingExceptionContextData).GetMethod("Create", Constants.PublicStatic);

        public static MappingExceptionData<TSource, TTarget> Create<TSource, TTarget>(
            MappingData<TSource, TTarget> data,
            Exception exception)
            => new MappingExceptionData<TSource, TTarget>(data, exception);
    }

    internal class MappingExceptionData<TSource, TTarget> :
        MappingData<TSource, TTarget>,
        IMappingExceptionData,
        IMappingExceptionData<TSource, TTarget>
    {
        public MappingExceptionData(MappingData<TSource, TTarget> data, Exception exception)
            : base(data, data.MapperData)
        {
            Exception = exception;
        }

        object IMappingExceptionData.Source => Source;

        object IMappingExceptionData.Target => Target;

        public Exception Exception { get; }
    }
}