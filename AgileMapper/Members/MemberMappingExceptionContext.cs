namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal static class MemberMappingExceptionContext
    {
        public static MemberMappingExceptionContext<TSource, TTarget> Create<TSource, TTarget>(
            TypedMemberMappingContext<TSource, TTarget> context,
            Exception exception)
            => new MemberMappingExceptionContext<TSource, TTarget>(context, exception);
    }

    internal class MemberMappingExceptionContext<TSource, TTarget> :
        TypedMemberMappingContext<TSource, TTarget>,
        IUntypedMemberMappingExceptionContext
    {
        public MemberMappingExceptionContext(TypedMemberMappingContext<TSource, TTarget> context, Exception exception)
            : base(context.Source, context.Target, context.EnumerableIndex)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public new object Source => base.Source;

        public new object Target => base.Target;
    }
}