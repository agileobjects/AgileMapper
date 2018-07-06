namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class TryParseConverter<T> : IValueConverter
    {
        public static readonly TryParseConverter<T> Instance = new TryParseConverter<T>();

        private readonly Type _nonNullableTargetType;
        private readonly Type _nullableTargetType;
        private readonly MethodInfo _tryParseMethod;
        private readonly ParameterExpression _valueVariable;

        protected TryParseConverter()
        {
            _nonNullableTargetType = typeof(T);
            _nullableTargetType = typeof(Nullable<>).MakeGenericType(_nonNullableTargetType);

            _tryParseMethod = _nonNullableTargetType.GetPublicStaticMethod(
                "TryParse",
                typeof(string),
                _nonNullableTargetType.MakeByRefType());

            _valueVariable = Expression.Variable(
                _nonNullableTargetType,
                _nonNullableTargetType.GetVariableNameInCamelCase() + "Value");
        }

        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => nonNullableTargetType == _nonNullableTargetType && CanConvert(nonNullableSourceType);

        protected virtual bool CanConvert(Type nonNullableSourceType)
        {
            return (nonNullableSourceType == _nonNullableTargetType) ||
                    ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType);
        }

        public virtual Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == _nullableTargetType)
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
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