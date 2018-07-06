namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

        private static Expression GetStringConvertCall(
            Expression subject,
            Type sqlFunctionsType,
            bool includeDecimalPlaces = true)
        {
            var subjectType = subject.Type.GetNonNullableType();

            if (subjectType == typeof(DateTime))
            {
                return GetParseDateTimeToStringOrNull(subject, sqlFunctionsType);
            }

            return (subjectType == typeof(decimal))
                ? GetTrimmedStringConvertCall<decimal?>(sqlFunctionsType, subject, subjectType, includeDecimalPlaces)
                : GetTrimmedStringConvertCall<double?>(sqlFunctionsType, subject, subjectType, includeDecimalPlaces);
        }

        private static Expression GetTrimmedStringConvertCall<TSubject>(
            Type sqlFunctionsType,
            Expression subject,
            Type subjectType,
            bool includeDecimalPlaces)
        {
            if (includeDecimalPlaces)
            {
                includeDecimalPlaces = subjectType.IsNonWholeNumberNumeric();
            }

            subject = subject.GetConversionTo<TSubject>();

            Expression stringConvertCall;

            if (includeDecimalPlaces)
            {
                stringConvertCall = Expression.Call(
                    GetConvertMethod(sqlFunctionsType, typeof(TSubject), typeof(int?), typeof(int?)),
                    subject.GetConversionTo<TSubject>(),
                    20.ToConstantExpression(typeof(int?)),  // <-- Total Length
                    6.ToConstantExpression(typeof(int?))); // <-- Decimal places
            }
            else
            {
                stringConvertCall = Expression.Call(
                    GetConvertMethod(sqlFunctionsType, typeof(TSubject)),
                    subject);
            }

            var trimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var trimCall = Expression.Call(stringConvertCall, trimMethod);

            return trimCall;
        }

        private static MethodInfo GetConvertMethod(Type sqlFunctionsType, params Type[] argumentTypes)
            => sqlFunctionsType.GetPublicStaticMethod("StringConvert", argumentTypes);

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

            return GetStringConvertCall(datePartNameCall, sqlFunctionsType, false);
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