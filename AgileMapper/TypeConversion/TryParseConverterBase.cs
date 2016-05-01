namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class TryParseConverterBase : IValueConverter
    {
        private readonly Type _targetType;
        private readonly Type _nullableTargetType;

        protected TryParseConverterBase(Type targetType)
        {
            _targetType = targetType;
            _nullableTargetType = typeof(Nullable<>).MakeGenericType(targetType);
        }

        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType == _targetType;
        }

        public virtual bool CanConvert(Type sourceType)
        {
            return sourceType == _nullableTargetType;
        }

        public virtual Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == _nullableTargetType)
            {
                return Expression.Call(
                    sourceValue,
                    sourceValue.Type.GetMethod("GetValueOrDefault", Constants.NoTypeArguments));
            }

            var tryParseMethod = StringExtensions.GetTryParseMethodFor(targetType);
            var tryParseCall = Expression.Call(tryParseMethod, sourceValue);

            return tryParseCall;
        }
    }
}