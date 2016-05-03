namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class ToEnumConverter : IValueConverter
    {
        private readonly ToStringConverter _toStringConverter;

        public ToEnumConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
        }

        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType.IsEnum;
        }

        public bool CanConvert(Type nonNullableSourceType)
        {
            return nonNullableSourceType.IsEnum ||
                (nonNullableSourceType == typeof(string)) ||
                (nonNullableSourceType == typeof(char)) ||
                nonNullableSourceType.IsNumeric();
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var tryParseMethod = StringExtensions.GetTryParseEnumMethodFor(targetType);
            var tryParseCall = Expression.Call(tryParseMethod, sourceValue);

            return tryParseCall;
        }
    }
}