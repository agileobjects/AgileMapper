namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ExceptionCallback
    {
        public ExceptionCallback(Expression callback)
        {
            Callback = callback;
            IsUntyped = callback.Type == typeof(Action<IUntypedMemberMappingExceptionContext>);
        }

        public Expression Callback { get; }

        public bool IsUntyped { get; }
    }
}