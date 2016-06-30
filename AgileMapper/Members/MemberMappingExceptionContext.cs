namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Reflection;

    internal static class MemberMappingExceptionContext
    {
        internal static readonly MethodInfo CreateMethod = 
            typeof(MemberMappingExceptionContext).GetMethod("Create", Constants.PublicStatic);

        public static MemberMappingExceptionContext<TSource, TTarget> Create<TSource, TTarget>(
            TypedMemberMappingContext<TSource, TTarget> context,
            Exception exception)
            => new MemberMappingExceptionContext<TSource, TTarget>(context, exception);
    }

    internal class MemberMappingExceptionContext<TSource, TTarget> :
        TypedMemberMappingContext<TSource, TTarget>,
        ITypedMemberMappingExceptionContext<TSource, TTarget>,
        IUntypedMemberMappingExceptionContext
    {
        public MemberMappingExceptionContext(ITypedMemberMappingContext<TSource, TTarget> context, Exception exception)
            : base(context.Source, context.Target, context.EnumerableIndex)
        {
            Exception = exception;
        }

        object IUntypedMemberMappingExceptionContext.Source => Source;

        object IUntypedMemberMappingExceptionContext.Target => Target;

        public Exception Exception { get; }
    }
}