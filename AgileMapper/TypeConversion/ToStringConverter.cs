namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
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
                return GetBase64StringToByteArrayConversion(sourceValue);
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

        private static Expression GetBase64StringToByteArrayConversion(Expression sourceValue)
        {
            return Expression.Call(_toBase64String, sourceValue);
        }

        #endregion
    }
}