namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Extensions;
    using Settings;

    internal static class ToStringConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProviderSettings settings,
            out Expression converted)
        {
            if (settings.SupportsToString || IsNotToStringCall(methodCall))
            {
                converted = null;
                return false;
            }

            converted = settings.ConvertToString(methodCall);
            return true;
        }

        private static bool IsNotToStringCall(MethodCallExpression methodCall)
        {
            return methodCall.Arguments.Any() ||
                   methodCall.Method.IsStatic ||
                  (methodCall.Method.Name != "ToString");
        }
    }
}