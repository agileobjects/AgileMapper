namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal static class QueryProviderSettingsExtensions
    {
        public static Expression GetConvertToTypeCall(
            this IQueryProviderSettings settings,
            MethodCallExpression tryParseCall)
        {
            // ReSharper disable once PossibleNullReferenceException
            // Attempt to use Convert.ToInt32 - irretrievably unsupported in non-EDMX EF5 and EF6, 
            // but it at least gives a decent error message:
            var convertMethodName = "To" + tryParseCall.Method.DeclaringType.Name;

            var convertMethod = typeof(Convert)
                .GetPublicStaticMethods(convertMethodName)
                .First(m => m.GetParameters().HasOne() && (m.GetParameters()[0].ParameterType == typeof(string)));

            var convertCall = Expression.Call(convertMethod, tryParseCall.Arguments.First());

            return convertCall;
        }

#if !NET_STANDARD
        public static bool TryGetDateTimeFromStringCall(
            this IQueryProviderSettings settings,
            MethodCallExpression tryParseCall,
            Expression fallbackValue,
            out Expression convertedCall)
        {
            if ((tryParseCall.Method.DeclaringType != typeof(DateTime)) ||
                (settings.CanonicalFunctionsType == null) ||
                (settings.SqlFunctionsType == null))
            {
                convertedCall = null;
                return false;
            }

            var createDateTimeMethod = settings
                .CanonicalFunctionsType
                .GetPublicStaticMethod("CreateDateTime");

            if (createDateTimeMethod == null)
            {
                convertedCall = null;
                return false;
            }

            var datePartMethod = settings
                .SqlFunctionsType
                .GetPublicStaticMethods("DatePart")
                .FirstOrDefault(m => m.GetParameters().All(p => p.ParameterType == typeof(string)));

            if (datePartMethod == null)
            {
                convertedCall = null;
                return false;
            }

            var sourceValue = tryParseCall.Arguments[0];

            var createDateTimeCall = Expression.Call(
                createDateTimeMethod,
                GetDatePartCall(datePartMethod, "yy", sourceValue),
                GetDatePartCall(datePartMethod, "mm", sourceValue),
                GetDatePartCall(datePartMethod, "dd", sourceValue),
                GetDatePartCall(datePartMethod, "hh", sourceValue),
                GetDatePartCall(datePartMethod, "mi", sourceValue),
                GetDatePartCall(datePartMethod, "ss", sourceValue).GetConversionTo(typeof(double?)));

            if (fallbackValue.NodeType == ExpressionType.Default)
            {
                fallbackValue = DefaultExpressionConverter.Convert((DefaultExpression)fallbackValue);
            }

            var createdDateTime = GetGuardedDateCreation(createDateTimeCall, sourceValue, fallbackValue, settings);

            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(sourceValue, nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, createdDateTime, fallbackValue);

            convertedCall = convertedOrFallback;
            return true;
        }

        private static Expression GetDatePartCall(
            MethodInfo datePartMethod,
            string datePart,
            Expression sourceValue)
        {
            return Expression.Call(datePartMethod, datePart.ToConstantExpression(), sourceValue);
        }

        private static Expression GetGuardedDateCreation(
            Expression createDateTimeCall,
            Expression sourceValue,
            Expression fallbackValue,
            IQueryProviderSettings settings)
        {
            var createdDateTime = Expression.Property(createDateTimeCall, "Value");

            var isDateMethod = settings.SqlFunctionsType.GetPublicStaticMethod("IsDate");

            if (isDateMethod == null)
            {
                return createDateTimeCall;
            }

            var isDateCall = Expression.Call(isDateMethod, sourceValue);
            var one = 1.ToConstantExpression(typeof(int?));
            var isDateIsTrue = Expression.Equal(isDateCall, one);
            var createdDateOrFallback = Expression.Condition(isDateIsTrue, createdDateTime, fallbackValue);

            return createdDateOrFallback;
        }
#endif
    }
}