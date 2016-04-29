namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal class ToStringConverter : IValueConverter
    {
        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType == typeof(string);
        }

        public bool CanConvert(Type sourceType)
        {
            return true;
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(byte[]))
            {
                return GetBase64StringToByteArrayConversion(sourceValue);
            }

            var toStringMethod = sourceValue.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => m.Name == "ToString");

            var toStringCall = Expression.Call(sourceValue, toStringMethod);

            return toStringCall;
        }

        #region Byte[] Conversion

        private static readonly MethodInfo _toBase64String = typeof(Convert)
            .GetMethods(Constants.PublicStatic)
            .First(m => (m.Name == "ToBase64String") && m.GetParameters().HasOne());

        private static Expression GetBase64StringToByteArrayConversion(Expression sourceValue)
        {
            return Expression.Call(_toBase64String, sourceValue);
        }

        #endregion
    }
}