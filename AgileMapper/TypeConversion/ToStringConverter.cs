namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal class ToStringConverter : ValueConverterBase
    {
        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType) => nonNullableTargetType == typeof(string);

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            // Target type is always 'string':
            return GetConversion(sourceValue);
        }

        public Expression GetConversion(Expression sourceValue)
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

            var toStringMethod = sourceValue.Type
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "ToString") && m.GetParameters().None());

            var toStringCall = Expression.Call(sourceValue, toStringMethod);

            return toStringCall;
        }

        #region Byte[] Conversion

        private static readonly MethodInfo _toBase64String = typeof(Convert)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "ToBase64String") && m.GetParameters().HasOne());

        private static Expression GetByteArrayToBase64StringConversion(Expression sourceValue)
        {
            return Expression.Call(_toBase64String, sourceValue);
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
                .GetPublicInstanceMethods()
                .Where(m => m.Name == "ToString")
                .Select(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .FirstOrDefault(m => m.Parameters.HasOne() && (m.Parameters[0].ParameterType == argumentType))?
                .Method;

            return toStringMethod;
        }
    }
}