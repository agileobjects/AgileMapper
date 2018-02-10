﻿namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using NetStandardPolyfills;

    internal static class StringToEnumConversionConverter
    {
        public static bool TryConvert(
            ConditionalExpression conditional,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (modifier.Settings.SupportsStringToEnumConversion || IsNotStringToEnumConversion(conditional))
            {
                converted = null;
                return false;
            }

            converted = modifier.Settings.ConvertStringToEnumConversion(conditional);
            return true;
        }

        private static bool IsNotStringToEnumConversion(ConditionalExpression conditional)
        {
            if (!conditional.Type.IsEnum() ||
                (conditional.Test.NodeType != ExpressionType.Call))
            {
                return true;
            }

            var testMethodCall = (MethodCallExpression)conditional.Test;

            return !testMethodCall.Method.IsStatic ||
                   (testMethodCall.Method.DeclaringType != typeof(string)) ||
                    testMethodCall.Method.Name != nameof(string.IsNullOrWhiteSpace);
        }
    }
}