namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class TryParseConverterBase : IValueConverter
    {
        private readonly ToStringConverter _toStringConverter;
        private readonly Type _targetType;
        private readonly Type _nullableTargetType;

        protected TryParseConverterBase(
            ToStringConverter toStringConverter,
            Type targetType)
        {
            _toStringConverter = toStringConverter;
            _targetType = targetType;
            _nullableTargetType = typeof(Nullable<>).MakeGenericType(targetType);
        }

        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType == _targetType;
        }

        public virtual bool CanConvert(Type nonNullableSourceType)
        {
            return nonNullableSourceType == _targetType;
        }

        public virtual Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == _nullableTargetType)
            {
                return sourceValue.GetToValueOrDefaultCall();
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var tryParseMethod = StringExtensions.GetTryParseMethodFor(targetType);
            var tryParseCall = Expression.Call(tryParseMethod, sourceValue);

            return tryParseCall;
        }
    }
}