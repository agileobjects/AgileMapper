namespace AgileObjects.AgileMapper.Members
{
    using System;

    public interface IUntypedMemberMappingExceptionContext
    {
        object Source { get; }

        object Target { get; }

        int? EnumerableIndex { get; }

        Exception Exception { get; }
    }
}