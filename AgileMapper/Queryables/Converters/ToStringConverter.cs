namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal static class ToStringConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (IsNotToStringCall(methodCall))
            {
                converted = null;
                return false;
            }

            if (modifier.Settings.SupportsToString)
            {
                converted = AdjustForFormatStringIfNecessary(methodCall, modifier);
                return converted != methodCall;
            }

            methodCall = AdjustForFormatStringIfNecessary(methodCall, modifier);
            converted = modifier.Settings.ConvertToStringCall(methodCall);
            return true;
        }

        private static bool IsNotToStringCall(MethodCallExpression methodCall)
            => methodCall.Method.IsStatic || (methodCall.Method.Name != nameof(ToString));

        private static MethodCallExpression AdjustForFormatStringIfNecessary(
            MethodCallExpression methodCall,
            IQueryProjectionModifier context)
        {
            if (context.Settings.SupportsToStringWithFormat || methodCall.Arguments.None())
            {
                return methodCall;
            }

            return methodCall.Object.WithToStringCall();
        }
    }
}