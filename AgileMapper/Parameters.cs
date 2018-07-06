namespace AgileObjects.AgileMapper
{
    using System;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class Parameters
    {
        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());
    }
}