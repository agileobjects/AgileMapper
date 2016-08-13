namespace AgileObjects.AgileMapper.Members
{
    using System;

    public interface IMappingExceptionData
    {
        object Source { get; }

        object Target { get; }

        int? EnumerableIndex { get; }

        Exception Exception { get; }
    }

    public interface IMappingExceptionData<out TSource, out TTarget>
        : IMappingData<TSource, TTarget>
    {
        Exception Exception { get; }
    }
}