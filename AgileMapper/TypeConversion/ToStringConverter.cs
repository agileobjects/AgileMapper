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

            if (sourceValue.Type == typeof(DateTime))
            {
                return GetDateTimeToStringConversion(sourceValue);
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

        private static Expression GetDateTimeToStringConversion(Expression sourceValue)
        {
            var toStringMethod = sourceValue.Type   
                .GetPublicInstanceMethods()
                .Where(m => m.Name == "ToString")
                .Select(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .First(m => m.Parameters.HasOne() &&
                           (m.Parameters[0].ParameterType == typeof(IFormatProvider)))
                .Method;

            var currentCulture = Expression.Property(null, typeof(CultureInfo), "CurrentCulture");
            var dateTimeFormat = Expression.Property(currentCulture, typeof(CultureInfo), "DateTimeFormat");

            var toStringCall = Expression.Call(sourceValue, toStringMethod, dateTimeFormat);

            return toStringCall;
        }
    }
}