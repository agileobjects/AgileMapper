namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class ExceptionCallback
    {
        public ExceptionCallback(Expression callback)
        {
            Callback = callback;
            IsUntyped = callback.Type == typeof(Action<IUntypedMemberMappingExceptionContext>);

            if (IsUntyped)
            {
                return;
            }

            var callbackSourceType = callback.Type.GetGenericArguments().First();
            IsSourceTyped = callbackSourceType != typeof(object);
        }

        public Expression Callback { get; }

        public bool IsUntyped { get; }

        public bool IsSourceTyped { get; }
    }
}