namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ToFormattedStringConverter : ValueConverterBase
    {
        private readonly Type _sourceValueType;
        private readonly MethodInfo _toStringMethod;
        private readonly ConstantExpression _formattingString;

        public ToFormattedStringConverter(Type sourceValueType, string formattingString)
        {
            _toStringMethod = ToStringConverter.GetToStringMethodOrNull(sourceValueType, typeof(string));

            if (_toStringMethod == null)
            {
                throw new NotSupportedException(
                    "No ToString method taking a formatting string exists on type " + sourceValueType.GetFriendlyName());
            }

            _sourceValueType = sourceValueType;
            _formattingString = formattingString.ToConstantExpression(typeof(string));
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return (nonNullableTargetType == typeof(string)) && (_sourceValueType == nonNullableSourceType);
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var toStringCall = Expression.Call(sourceValue, _toStringMethod, _formattingString);

            return toStringCall;
        }
    }
}