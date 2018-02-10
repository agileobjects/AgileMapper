namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;

    internal static class StringEqualsIgnoreCaseConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (modifier.Settings.SupportsStringEqualsIgnoreCase || IsNotEqualsIgnoreCaseCall(methodCall))
            {
                converted = null;
                return false;
            }

            converted = modifier.Settings.ConvertStringEqualsIgnoreCase(methodCall);
            return true;
        }

        private static bool IsNotEqualsIgnoreCaseCall(MethodCallExpression methodCall)
        {
            return !methodCall.Method.IsStatic ||
                   (methodCall.Arguments.Count != 3) ||
                   (methodCall.Method.DeclaringType != typeof(string)) ||
                   (methodCall.Method.Name != nameof(string.Equals));
        }
    }
}