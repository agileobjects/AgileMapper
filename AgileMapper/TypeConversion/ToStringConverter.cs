namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
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

            if (HasToStringOperator(nonNullableSourceType, out var operatorMethod))
            {
                return Expression.Call(operatorMethod, sourceValue);
            }

            var toStringMethod = sourceValue.Type
                .GetPublicInstanceMethod("ToString", parameterCount: 0);

            var toStringCall = Expression.Call(sourceValue, toStringMethod);

            return toStringCall;
        }

        public bool HasToStringOperator(Type nonNullableSourceType, out MethodInfo operatorMethod)
        {
            operatorMethod = nonNullableSourceType
                .GetPublicStaticMembers()
                .Where(m => (m.Name == "op_Implicit") || (m.Name == "op_Explicit"))
                .Cast<MethodInfo>()
                .FirstOrDefault(m =>
                    (m.ReturnType == typeof(string)) &&
                    (m.GetParameters()[0].ParameterType == nonNullableSourceType));

            return operatorMethod != null;
        }

        #region Byte[] Conversion

        private static readonly MethodInfo _toBase64String = typeof(Convert)
            .GetPublicStaticMethod("ToBase64String", parameterCount: 1);

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
                .GetPublicInstanceMethods("ToString")
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