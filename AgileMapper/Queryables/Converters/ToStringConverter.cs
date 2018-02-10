namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal static class ToStringConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier context,
            out Expression converted)
        {
            if (context.Settings.SupportsToString || IsNotToStringCall(methodCall))
            {
                converted = null;
                return false;
            }

            converted = context.Settings.ConvertToStringCall(methodCall);
            return true;
        }

        private static bool IsNotToStringCall(MethodCallExpression methodCall)
        {
            return methodCall.Arguments.Any() ||
                   methodCall.Method.IsStatic ||
                  (methodCall.Method.Name != nameof(ToString));
        }
    }
}