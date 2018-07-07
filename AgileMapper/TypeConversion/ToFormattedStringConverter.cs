namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Reflection;
    using Configuration;
    using Extensions.Internal;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ToFormattedStringConverter : IValueConverter
    {
        private readonly Type _sourceValueType;
        private readonly MethodInfo _toStringMethod;
        private readonly ConstantExpression _formattingString;

        public ToFormattedStringConverter(Type sourceValueType, string formattingString)
        {
            _toStringMethod = ToStringConverter.GetToStringMethodOrNull(sourceValueType, typeof(string));

            if (_toStringMethod == null)
            {
                throw new MappingConfigurationException(
                    "No ToString method taking a formatting string exists on type " + sourceValueType.GetFriendlyName());
            }

            _sourceValueType = sourceValueType;
            _formattingString = formattingString.ToConstantExpression(typeof(string));
        }

        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => (nonNullableTargetType == typeof(string)) && (_sourceValueType == nonNullableSourceType);

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var toStringCall = Expression.Call(sourceValue, _toStringMethod, _formattingString);

            return toStringCall;
        }
    }
}