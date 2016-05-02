namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class ToEnumConverter : IValueConverter
    {
        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType.IsEnum;
        }

        public bool CanConvert(Type sourceType)
        {
            return sourceType.IsEnum ||
                (sourceType == typeof(string)) ||
                sourceType.IsNumeric();
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type.IsNumeric())
            {
                return sourceValue.GetConversionTo(targetType);
            }

            if (sourceValue.Type == typeof(string))
            {
                var tryParseMethod = StringExtensions.GetTryParseEnumMethodFor(targetType);
                var tryParseCall = Expression.Call(tryParseMethod, sourceValue);

                return tryParseCall;
            }

            return sourceValue;
        }
    }
}