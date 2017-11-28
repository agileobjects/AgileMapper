namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;
    using Settings;

    internal static class GetValueOrDefaultConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProviderSettings settings,
            out Expression converted)
        {
            if (settings.SupportsGetValueOrDefault || IsNotGetValueOrDefaultCall(methodCall))
            {
                converted = null;
                return false;
            }

            converted = settings.ConvertGetValueOrDefaultCall(methodCall);
            return true;
        }

        private static bool IsNotGetValueOrDefaultCall(MethodCallExpression methodCall)
        {
            return methodCall.Arguments.Any() ||
                   methodCall.Method.IsStatic ||
                  !methodCall.Object.Type.IsNullableType() ||
                  (methodCall.Method.Name != "GetValueOrDefault");
        }
    }
}