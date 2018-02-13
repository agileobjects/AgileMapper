namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class Ef5QueryProviderSettings : LegacyEfQueryProviderSettings
    {
        public override bool SupportsToString => false;

        public override bool SupportsNonEntityNullConstants => false;

        protected override Type LoadCanonicalFunctionsType()
        {
            return GetTypeOrNull(
                "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Objects.EntityFunctions");
        }

        protected override Type LoadSqlFunctionsType()
        {
            return GetTypeOrNull(
                "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.Objects.SqlClient.SqlFunctions");
        }

        public override Expression ConvertToStringCall(MethodCallExpression call)
        {
            var sqlFunctionsType = SqlFunctionsType;

            if (sqlFunctionsType == null)
            {
                return base.ConvertToStringCall(call);
            }

            return GetStringConvertCall(call.Object, sqlFunctionsType);
        }

        private static Expression GetStringConvertCall(Expression subject, Type sqlFunctionsType)
        {
            var subjectType = subject.Type.GetNonNullableType();

            if (subjectType == typeof(DateTime))
            {
                return GetParseDateTimeToStringOrNull(subject, sqlFunctionsType);
            }

            if (subjectType == typeof(decimal))
            {
                return GetTrimmedStringConvertCall<decimal?>(sqlFunctionsType, subject);
            }

            if (subjectType != typeof(double))
            {
                subject = Expression.Convert(subject, typeof(double?));
            }

            return GetTrimmedStringConvertCall<double?>(sqlFunctionsType, subject);
        }

        private static Expression GetTrimmedStringConvertCall<TArgument>(Type sqlFunctionsType, Expression subject)
        {
            var stringConvertCall = Expression.Call(
                sqlFunctionsType.GetPublicStaticMethod("StringConvert", typeof(TArgument)),
                subject);

            var trimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var trimCall = Expression.Call(stringConvertCall, trimMethod);

            return trimCall;
        }

        private static Expression GetParseDateTimeToStringOrNull(Expression dateValue, Type sqlFunctionsType)
        {
            if (!TryGetDatePartMethod<DateTime?>(sqlFunctionsType, out var datePartMethod))
            {
                return null;
            }

            dateValue = dateValue.GetConversionTo<DateTime?>();

            var dateTimePattern = CultureInfo
                .CurrentCulture
                .DateTimeFormat
                .SortableDateTimePattern
                .Replace("mm", "mi") // Minutes
                .Replace('T', ' ')   // Date-Time separator
                .Replace("'", null)
                .ToLowerInvariant();

            Expression valueConcatenation = null;
            var datePartStartIndex = 0;
            var stringConcat = StringExpressionExtensions.GetConcatMethod(parameterCount: 2);

            for (var i = 0; i < dateTimePattern.Length; ++i)
            {
                if (char.IsLetter(dateTimePattern[i]))
                {
                    continue;
                }

                var datePartNameCall = GetDatePartCall(
                    datePartMethod,
                    dateTimePattern,
                    datePartStartIndex,
                    i,
                    dateValue,
                    sqlFunctionsType);

                var added = Expression.Call(
                    stringConcat,
                    datePartNameCall,
                    dateTimePattern[i].ToString().ToConstantExpression());

                valueConcatenation = (valueConcatenation != null)
                    ? Expression.Call(stringConcat, valueConcatenation, added) : added;

                datePartStartIndex = i + 1;
            }

            var finalDatePartNameCall = GetDatePartCall(
                datePartMethod,
                dateTimePattern,
                datePartStartIndex,
                dateTimePattern.Length,
                dateValue,
                sqlFunctionsType);

            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.Call(stringConcat, valueConcatenation, finalDatePartNameCall);
        }

        private static Expression GetDatePartCall(
            MethodInfo datePartMethod,
            string dateTimePattern,
            int datePartStartIndex,
            int datePartEndIndex,
            Expression dateValue,
            Type sqlFunctionsType)
        {
            var datePartNameCall = GetDatePartCall(
                datePartMethod,
                GetDatePart(dateTimePattern, datePartStartIndex, datePartEndIndex),
                dateValue);

            return GetStringConvertCall(datePartNameCall, sqlFunctionsType);
        }

        private static string GetDatePart(
            string dateTimePattern,
            int datePartStartIndex,
            int datePartEndIndex)
        {
            return dateTimePattern.Substring(datePartStartIndex, datePartEndIndex - datePartStartIndex);
        }
    }
}