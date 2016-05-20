namespace AgileObjects.AgileMapper.Members
{
    using System;

    public interface ITypedMemberMappingExceptionContext<out TSource, out TTarget>
        : ITypedMemberMappingContext<TSource, TTarget>
    {
        Exception Exception { get; }
    }
}