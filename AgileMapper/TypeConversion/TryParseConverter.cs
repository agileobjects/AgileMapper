namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class TryParseConverter<T> : ValueConverterBase
    {
        private readonly ToStringConverter _toStringConverter;
        private readonly Type _nonNullableTargetType;
        private readonly Type _nullableTargetType;
        private readonly MethodInfo _tryParseMethod;
        private readonly ParameterExpression _valueVariable;

        public TryParseConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
            _nonNullableTargetType = typeof(T);
            _nullableTargetType = typeof(Nullable<>).MakeGenericType(_nonNullableTargetType);

            _tryParseMethod = _nonNullableTargetType
                .GetPublicStaticMethod("TryParse", parameterCount: 2);

            _valueVariable = Expression.Variable(
                _nonNullableTargetType,
                _nonNullableTargetType.GetVariableNameInCamelCase() + "Value");
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => nonNullableTargetType == _nonNullableTargetType && CanConvert(nonNullableSourceType);

        protected virtual bool CanConvert(Type nonNullableSourceType)
        {
            return (nonNullableSourceType == _nonNullableTargetType) ||
                   _toStringConverter.HasNativeStringRepresentation(nonNullableSourceType);
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == _nullableTargetType)
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var tryParseCall = Expression.Call(_tryParseMethod, sourceValue, _valueVariable);
            var successfulParseReturnValue = _valueVariable.GetConversionTo(targetType);
            var defaultValue = targetType.ToDefaultExpression();
            var parsedValueOrDefault = Expression.Condition(tryParseCall, successfulParseReturnValue, defaultValue);
            var tryParseBlock = Expression.Block(new[] { _valueVariable }, parsedValueOrDefault);

            return tryParseBlock;
        }
    }
}