namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct ToStringConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => nonNullableTargetType == typeof(string);

        public static bool HasNativeStringRepresentation(Type nonNullableType)
        {
            return (nonNullableType == typeof(string)) ||
                   (nonNullableType == typeof(object)) ||
                    nonNullableType.IsEnum() ||
                   (nonNullableType == typeof(char)) ||
                    HasToStringOperator(nonNullableType, out _);
        }

        private static bool HasToStringOperator(Type nonNullableSourceType, out MethodInfo operatorMethod)
        {
            operatorMethod = nonNullableSourceType
                .GetOperators(o => o.To<string>())
                .FirstOrDefault();

            return operatorMethod != null;
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            // Target type is always 'string':
            return GetConversion(sourceValue);
        }

        public static Expression GetConversion(Expression sourceValue)
        {
            if (sourceValue.Type == typeof(byte[]))
            {
                return GetByteArrayToBase64StringConversion(sourceValue);
            }

            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();

            if (nonNullableSourceType == typeof(DateTime))
            {
                return GetDateTimeToStringConversion(sourceValue, nonNullableSourceType);
            }

            if (nonNullableSourceType == typeof(bool))
            {
                return GetBoolToStringConversion(sourceValue, nonNullableSourceType);
            }

            if (HasToStringOperator(nonNullableSourceType, out var operatorMethod))
            {
                return Expression.Call(operatorMethod, sourceValue);
            }

            return sourceValue.WithToStringCall();
        }

        #region Byte[] Conversion

        private static Expression GetByteArrayToBase64StringConversion(Expression sourceValue)
        {
            return Expression.Call(
                typeof(Convert).GetPublicStaticMethod(nameof(Convert.ToBase64String), parameterCount: 1),
                sourceValue);
        }

        #endregion

        private static Expression GetDateTimeToStringConversion(Expression sourceValue, Type nonNullableSourceType)
        {
            var toStringMethod = GetToStringMethodOrNull(nonNullableSourceType, typeof(IFormatProvider));
            var currentCulture = Expression.Property(null, typeof(CultureInfo), "CurrentCulture");
            var dateTimeFormat = Expression.Property(currentCulture, typeof(CultureInfo), "DateTimeFormat");

            if (sourceValue.Type != nonNullableSourceType)
            {
                sourceValue = Expression.Property(sourceValue, "Value");
            }

            var toStringCall = Expression.Call(sourceValue, toStringMethod, dateTimeFormat);

            return toStringCall;
        }

        public static MethodInfo GetToStringMethodOrNull(Type sourceType, Type argumentType)
        {
            var toStringMethod = sourceType
                .GetPublicInstanceMethods("ToString")
                .Project(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .FirstOrDefault(m => m.Parameters.HasOne() && (m.Parameters[0].ParameterType == argumentType))?
                .Method;

            return toStringMethod;
        }

        private static Expression GetBoolToStringConversion(Expression sourceValue, Type nonNullableSourceType)
        {
            if (sourceValue.Type == nonNullableSourceType)
            {
                return GetTrueOrFalseTernary(sourceValue);
            }

            var nullTrueOrFalse = Expression.Condition(
                Expression.Property(sourceValue, "HasValue"),
                GetTrueOrFalseTernary(Expression.Property(sourceValue, "Value")),
                typeof(string).ToDefaultExpression());

            return nullTrueOrFalse;
        }

        private static Expression GetTrueOrFalseTernary(Expression sourceValue)
        {
            return Expression.Condition(
                sourceValue,
                "true".ToConstantExpression(),
                "false".ToConstantExpression());
        }
    }
}