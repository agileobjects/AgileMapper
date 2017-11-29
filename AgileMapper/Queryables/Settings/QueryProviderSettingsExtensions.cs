namespace AgileObjects.AgileMapper.Queryables.Settings
{
#if !NET_STANDARD
    using System.Linq.Expressions;
    using System.Linq;
    using System.Reflection;
    using Converters;
    using Extensions;
    using NetStandardPolyfills;
#endif
    using ObjectPopulation;

    internal static class QueryProviderSettingsExtensions
    {
        public static IQueryProviderSettings GetQueryProviderSettings(this IObjectMappingData mappingData)
        {
            while (!mappingData.IsRoot)
            {
                mappingData = mappingData.Parent;
            }

            var queryProviderType = ((QueryProjectorKey)mappingData.MapperKey).QueryProviderType;
            var providerSettings = QueryProviderSettings.For(queryProviderType);

            return providerSettings;
        }

#if !NET_STANDARD
        public static Expression GetCreateDateTimeFromStringOrNull(
            this IQueryProviderSettings settings,
            MethodCallExpression dateTimeTryParseCall,
            Expression fallbackValue)
        {
            if ((settings.CanonicalFunctionsType == null) || (settings.SqlFunctionsType == null))
            {
                return null;
            }

            var createDateTimeMethod = settings
                .CanonicalFunctionsType
                .GetPublicStaticMethod("CreateDateTime");

            if (createDateTimeMethod == null)
            {
                return null;
            }

            var datePartMethod = settings
                .SqlFunctionsType
                .GetPublicStaticMethods("DatePart")
                .FirstOrDefault(m => m.GetParameters().All(p => p.ParameterType == typeof(string)));

            if (datePartMethod == null)
            {
                return null;
            }

            var sourceValue = dateTimeTryParseCall.Arguments[0];

            var createDateTimeCall = Expression.Call(
                createDateTimeMethod,
                GetDatePartCall(datePartMethod, "yy", sourceValue),
                GetDatePartCall(datePartMethod, "mm", sourceValue),
                GetDatePartCall(datePartMethod, "dd", sourceValue),
                GetDatePartCall(datePartMethod, "hh", sourceValue),
                GetDatePartCall(datePartMethod, "mi", sourceValue),
                GetDatePartCall(datePartMethod, "ss", sourceValue).GetConversionTo<double?>());

            if (fallbackValue.NodeType == ExpressionType.Default)
            {
                fallbackValue = DefaultExpressionConverter.Convert(fallbackValue);
            }

            var createdDateTime = GetGuardedDateCreation(createDateTimeCall, sourceValue, fallbackValue, settings);

            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(sourceValue, nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, createdDateTime, fallbackValue);

            return convertedOrFallback;
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