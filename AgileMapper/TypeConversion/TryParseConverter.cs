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

    internal class TryParseConverter : IValueConverter
    {
        public static readonly TryParseConverter Instance = new TryParseConverter();

        public virtual bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return (GetTryParseMethod(nonNullableTargetType) != null) &&
                    ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType);
        }

        public virtual Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var nonNullableTargetType = targetType.GetNonNullableType();
            var tryParseMethod = GetTryParseMethod(nonNullableTargetType);
            var valueVariable = GetValueVariable(nonNullableTargetType);

            return GetConversion(sourceValue, targetType, tryParseMethod, valueVariable);
        }

        protected static Expression GetConversion(
            Expression sourceValue,
            Type targetType,
            MethodInfo tryParseMethod,
            ParameterExpression valueVariable)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            var tryParseCall = Expression.Call(tryParseMethod, sourceValue, valueVariable);
            var successfulParseReturnValue = valueVariable.GetConversionTo(targetType);
            var defaultValue = targetType.ToDefaultExpression();
            var parsedValueOrDefault = Expression.Condition(tryParseCall, successfulParseReturnValue, defaultValue);
            var tryParseBlock = Expression.Block(new[] { valueVariable }, parsedValueOrDefault);

            return tryParseBlock;
        }

        protected static ParameterExpression GetValueVariable(Type nonNullableTargetType)
        {
            return Expression.Variable(
                nonNullableTargetType,
                nonNullableTargetType.GetVariableNameInCamelCase() + "Value");
        }

        protected static MethodInfo GetTryParseMethod(Type nonNullableTargetType)
        {
            return nonNullableTargetType.GetPublicStaticMethod(
                "TryParse",
                typeof(string),
                nonNullableTargetType.MakeByRefType());
        }
    }

    internal class TryParseConverter<T> : TryParseConverter
    {
        public new static readonly TryParseConverter<T> Instance = new TryParseConverter<T>();

        private readonly Type _nonNullableTargetType;
        private readonly Type _nullableTargetType;
        private readonly MethodInfo _tryParseMethod;
        private readonly ParameterExpression _valueVariable;

        protected TryParseConverter()
        {
            _nonNullableTargetType = typeof(T);
            _nullableTargetType = typeof(Nullable<>).MakeGenericType(_nonNullableTargetType);

            _tryParseMethod = GetTryParseMethod(_nonNullableTargetType);
            _valueVariable = GetValueVariable(_nonNullableTargetType);
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => nonNullableTargetType == _nonNullableTargetType && CanConvert(nonNullableSourceType);

        protected virtual bool CanConvert(Type nonNullableSourceType)
        {
            return (nonNullableSourceType == _nonNullableTargetType) ||
                    ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType);
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == _nullableTargetType)
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            return GetConversion(sourceValue, targetType, _tryParseMethod, _valueVariable);
        }
    }
}