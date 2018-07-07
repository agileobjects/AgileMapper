namespace AgileObjects.AgileMapper.Queryables.Converters
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            converted = modifier.Modify(converted);
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