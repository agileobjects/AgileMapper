namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class ToStringConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType) => nonNullableTargetType == typeof(string);

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            // targetType is always 'string':
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
                .First(m => m.Name == "ToString");

            Expression toStringCall = Expression.Call(sourceValue, toStringMethod);

            if (!sourceValue.Type.IsNullableType() && sourceValue.Type.CanBeNull())
            {
                toStringCall = Expression.Condition(
                    sourceValue.GetIsNotDefaultComparison(),
                    toStringCall,
                    Expression.Default(typeof(string)));
            }

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