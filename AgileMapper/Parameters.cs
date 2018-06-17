namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal static class Parameters
    {
        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());
    }
}