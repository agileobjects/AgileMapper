namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class LinqExtensions
    {
        public static readonly MethodInfo LinqToArrayMethod;
        private static readonly MethodInfo _linqToListMethod;

        static LinqExtensions()
        {
            var linqEnumerableMethods = typeof(Enumerable).GetPublicStaticMethods().ToArray();
            LinqToArrayMethod = linqEnumerableMethods.First(m => m.Name == "ToArray");
            _linqToListMethod = linqEnumerableMethods.First(m => m.Name == "ToList");
        }

        public static Expression WithToListLinqCall(this Expression enumerable, Type elementType)
            => enumerable.GetToEnumerableCall(_linqToListMethod, elementType);

        public static Expression WithToArrayLinqCall(this Expression enumerable, Type elementType)
            => enumerable.GetToEnumerableCall(LinqToArrayMethod, elementType);

        public static Expression WithOrderingLinqCall(
            this Expression enumerable,
            string orderingMethodName,
            ParameterExpression element,
            Expression orderMemberAccess)
        {
            var funcTypes = new[] { element.Type, orderMemberAccess.Type };

            var orderingMethod = typeof(Enumerable)
                .GetPublicStaticMethod(orderingMethodName, parameterCount: 2)
                .MakeGenericMethod(funcTypes);

            var orderLambda = Expression.Lambda(
                Expression.GetFuncType(funcTypes),
                orderMemberAccess,
                element);

            return Expression.Call(orderingMethod, enumerable, orderLambda);
        }
    }
}
