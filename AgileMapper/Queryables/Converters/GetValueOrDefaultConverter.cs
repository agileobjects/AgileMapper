namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class GetValueOrDefaultConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (modifier.Settings.SupportsGetValueOrDefault || IsNotGetValueOrDefaultCall(methodCall))
            {
                converted = null;
                return false;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            converted = Expression.Condition(
                methodCall.Object.GetIsNotDefaultComparison(),
                Expression.Convert(methodCall.Object, methodCall.Type),
                DefaultValueConstantExpressionFactory.CreateFor(methodCall));

            return true;
        }

        private static bool IsNotGetValueOrDefaultCall(MethodCallExpression methodCall)
        {
            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Arguments.Any() ||
                   methodCall.Method.IsStatic ||
                  !methodCall.Object.Type.IsNullableType() ||
                  (methodCall.Method.Name != nameof(Nullable<int>.GetValueOrDefault));
        }
    }
}