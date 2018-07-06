namespace AgileObjects.AgileMapper.Queryables.Settings.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using TypeConversion;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal abstract class LegacyEfQueryProviderSettings : DefaultQueryProviderSettings
    {
        public override bool SupportsStringToEnumConversion => false;

        public override bool SupportsGetValueOrDefault => false;

        public override bool SupportsEnumerableMaterialisation => false;

        protected override Expression GetParseStringToDateTimeOrNull(MethodCallExpression call, Expression fallbackValue)
        {
            if ((CanonicalFunctionsType == null) || (SqlFunctionsType == null))
            {
                return null;
            }

            var createDateTimeMethod = CanonicalFunctionsType.GetPublicStaticMethod("CreateDateTime");

            if (createDateTimeMethod == null)
            {
                return null;
            }

            if (!TryGetDatePartMethod<string>(SqlFunctionsType, out var datePartMethod))
            {
                return null;
            }

            var sourceValue = call.Arguments[0];

            var createDateTimeCall = Expression.Call(
                createDateTimeMethod,
                GetDatePartCall(datePartMethod, "yy", sourceValue),
                GetDatePartCall(datePartMethod, "mm", sourceValue),
                GetDatePartCall(datePartMethod, "dd", sourceValue),
                GetDatePartCall(datePartMethod, "hh", sourceValue),
                GetDatePartCall(datePartMethod, "mi", sourceValue),
                GetDatePartCall(datePartMethod, "ss", sourceValue).GetConversionTo<double?>());

            fallbackValue = GetDefaultValueFor(fallbackValue);

            var createdDateTime = GetGuardedDateCreation(createDateTimeCall, sourceValue, fallbackValue);

            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(sourceValue, nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, createdDateTime, fallbackValue);

            return convertedOrFallback;
        }

        #region GetCreateDateTimeFromStringOrNull Helpers

        protected static bool TryGetDatePartMethod<TArgument>(Type sqlFunctionsType, out MethodInfo datePartMethod)
        {
            datePartMethod = sqlFunctionsType
                .GetPublicStaticMethod("DatePart", typeof(string), typeof(TArgument));

            return datePartMethod != null;
        }

        protected static Expression GetDatePartCall(
            MethodInfo datePartMethod,
            string datePart,
            Expression sourceValue)
        {
            return Expression.Call(datePartMethod, datePart.ToConstantExpression(), sourceValue);
        }

        private Expression GetGuardedDateCreation(
            Expression createDateTimeCall,
            Expression sourceValue,
            Expression fallbackValue)
        {
            var createdDateTime = Expression.Property(createDateTimeCall, "Value");

            var isDateMethod = SqlFunctionsType.GetPublicStaticMethod("IsDate");

            if (isDateMethod == null)
            {
                return createDateTimeCall;
            }

            var isDateCall = Expression.Call(isDateMethod, sourceValue);
            var isDateIsTrue = Expression.Equal(isDateCall, ToNumericConverter<int>.One.GetConversionTo<int?>());
            var createdDateOrFallback = Expression.Condition(isDateIsTrue, createdDateTime, fallbackValue);

            return createdDateOrFallback;
        }

        #endregion

        public override Expression ConvertStringEqualsIgnoreCase(MethodCallExpression call)
        {
            var stringToLowerMethod = typeof(string)
                .GetPublicInstanceMethod("ToLower", parameterCount: 0);

            var subjectToLower = GetToLowerConversion(call.Arguments[0], stringToLowerMethod);
            var comparisonValue = GetToLowerConversion(call.Arguments[1], stringToLowerMethod);

            return Expression.Equal(subjectToLower, comparisonValue);
        }

        #region GetStringEqualsIgnoreCaseConversion Helpers

        private static Expression GetToLowerConversion(Expression value, MethodInfo stringToLowerMethod)
        {
            if (value.NodeType != ExpressionType.Constant)
            {
                return Expression.Call(value, stringToLowerMethod);
            }

            var stringValue = ((ConstantExpression)value).Value?.ToString().ToLowerInvariant();

            return stringValue.ToConstantExpression();

        }

        #endregion

        public override Expression ConvertStringToEnumConversion(ConditionalExpression conversion)
        {
            var enumType = conversion.Type;
            var underlyingEnumType = Enum.GetUnderlyingType(enumType);

            var enumNumericValuesByField = enumType.GetPublicStaticFields().ToDictionary(
                f => f,
                f => Convert.ChangeType(f.GetValue(null), underlyingEnumType).ToString().ToConstantExpression());

            var fallbackValue = GetDefaultValueFor(conversion.IfTrue);

            while (IsNotStringEqualsCheck(conversion.Test))
            {
                conversion = (ConditionalExpression)conversion.IfFalse;
            }

            var stringEqualsCheck = (MethodCallExpression)conversion.Test;
            var stringValue = stringEqualsCheck.Arguments.First();

            var stringToEnumConversions = new List<StringToEnumConversion>(enumNumericValuesByField.Count);

            while (true)
            {
                if (conversion.IfFalse.NodeType == ExpressionType.Default)
                {
                    break;
                }

                var conversionTest = ConvertStringEqualsIgnoreCase((MethodCallExpression)conversion.Test);

                stringToEnumConversions.Insert(0, new StringToEnumConversion(conversionTest, conversion.IfTrue));

                conversion = (ConditionalExpression)conversion.IfFalse;
            }

            var noParseConversion = stringToEnumConversions.Aggregate(
                fallbackValue,
                (conversionSoFar, stringToEnumConversion) =>
                {
                    var enumNumericValue = enumNumericValuesByField[stringToEnumConversion.EnumValueField];
                    var stringIsNumericValue = Expression.Equal(stringValue, enumNumericValue);

                    return Expression.Condition(
                        Expression.OrElse(stringIsNumericValue, stringToEnumConversion.Test),
                        stringToEnumConversion.EnumValue,
                        conversionSoFar);
                });

            return noParseConversion;
        }

        #region GetNoParseStringToEnumConversion Helpers

        private static bool IsNotStringEqualsCheck(Expression test)
        {
            if (test.NodeType != ExpressionType.Call)
            {
                return true;
            }

            var testMethodCall = (MethodCallExpression)test;

            return !testMethodCall.Method.IsStatic ||
                   (testMethodCall.Method.DeclaringType != typeof(string)) ||
                   (testMethodCall.Method.Name != nameof(string.Equals));
        }

        private class StringToEnumConversion
        {
            public StringToEnumConversion(Expression test, Expression enumValue)
            {
                Test = test;
                EnumValue = (MemberExpression)enumValue;
            }

            public Expression Test { get; }

            public MemberExpression EnumValue { get; }

            public FieldInfo EnumValueField => (FieldInfo)EnumValue.Member;
        }

        #endregion
    }
}