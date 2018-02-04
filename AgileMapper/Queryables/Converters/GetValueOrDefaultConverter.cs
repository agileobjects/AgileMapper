namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ReadableExpressions.Extensions;

    internal static class GetValueOrDefaultConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier context,
            out Expression converted)
        {
            if (context.Settings.SupportsGetValueOrDefault || IsNotGetValueOrDefaultCall(methodCall))
            {
                converted = null;
                return false;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            converted = Expression.Condition(
                methodCall.Object.GetIsNotDefaultComparison(),
                Expression.Convert(methodCall.Object, methodCall.Type),
                DefaultValueConstantExpressionFactory.CreateFor(methodCall.Type));

            return true;
        }

        private static bool IsNotGetValueOrDefaultCall(MethodCallExpression methodCall)
        {
            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Arguments.Any() ||
                   methodCall.Method.IsStatic ||
                  !methodCall.Object.Type.IsNullableType() ||
                  (methodCall.Method.Name != "GetValueOrDefault");
        }
    }
}