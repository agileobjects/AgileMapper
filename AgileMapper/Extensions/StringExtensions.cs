namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class StringExtensions
    {
        public static string ToPascalCase(this string value)
        {
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static string ToCamelCase(this string value)
        {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static TValue TryParse<TValue>(this string stringValue)
        {
            return DefaultTryParser<TValue>.Instance.Parse(stringValue);
        }

        public static TEnum TryParseEnum<TEnum>(this string stringValue)
        {
            return EnumTryParser<TEnum>.Instance.Parse(stringValue);
        }

        #region TryParser Classes

        private abstract class TryParserBase<TValue>
        {
            private readonly Func<string, TValue> _parser;

            protected TryParserBase(
                Func<Type, Expression, Expression, Expression> tryParseCallFactory)
            {
                var nonNullableValueType = typeof(TValue)
                    .GetNonNullableUnderlyingTypeIfAppropriate();

                var stringValueParameter = Parameters.Create<string>("stringValue");
                var valueVariable = Expression.Variable(nonNullableValueType, "value");
                var tryParseCall = tryParseCallFactory.Invoke(nonNullableValueType, stringValueParameter, valueVariable);

                var successfulParseReturnValue = valueVariable.GetConversionTo(typeof(TValue));

                var defaultValue = Expression.Default(typeof(TValue));
                var parsedValueOrDefault = Expression.Condition(tryParseCall, successfulParseReturnValue, defaultValue);
                var tryParseBlock = Expression.Block(new[] { valueVariable }, parsedValueOrDefault);
                var tryParseLambda = Expression.Lambda<Func<string, TValue>>(tryParseBlock, stringValueParameter);

                _parser = tryParseLambda.Compile();
            }

            public TValue Parse(string stringValue)
            {
                return _parser.Invoke(stringValue);
            }
        }

        private class DefaultTryParser<TValue> : TryParserBase<TValue>
        {
            public static readonly DefaultTryParser<TValue> Instance = new DefaultTryParser<TValue>();

            private DefaultTryParser()
                : base(GetNumericTryParseCall)
            {
            }

            private static Expression GetNumericTryParseCall(
                Type nonNullableNumericType,
                Expression stringValueParameter,
                Expression valueVariable)
            {
                var tryParseMethod = nonNullableNumericType
                    .GetMethods(Constants.PublicStatic)
                    .First(m => (m.Name == "TryParse") && (m.GetParameters().Length == 2));

                var tryParseCall = Expression.Call(tryParseMethod, stringValueParameter, valueVariable);

                return tryParseCall;
            }
        }

        private class EnumTryParser<TEnum> : TryParserBase<TEnum>
        {
            public static readonly EnumTryParser<TEnum> Instance = new EnumTryParser<TEnum>();

            private EnumTryParser()
                : base(GetEnumTryParseCall)
            {
            }

            private static Expression GetEnumTryParseCall(
                Type nonNullableEnumType,
                Expression stringValueParameter,
                Expression valueVariable)
            {
                var tryParseMethod = typeof(Enum)
                    .GetMethods(Constants.PublicStatic)
                    .First(m => (m.Name == "TryParse") && (m.GetParameters().Length == 3))
                    .MakeGenericMethod(nonNullableEnumType);

                var tryParseCall = Expression.Call(
                    tryParseMethod,
                    stringValueParameter,
                    Expression.Constant(true, typeof(bool)), // <- IgnoreCase
                    valueVariable);

                return tryParseCall;
            }
        }

        #endregion

        private static readonly MethodInfo _tryParseMethod =
            typeof(StringExtensions)
                .GetMethods(Constants.PublicStatic)
                .First(m => m.Name == "TryParse");

        private static readonly MethodInfo _tryParseEnumMethod =
            typeof(StringExtensions)
                .GetMethods(Constants.PublicStatic)
                .First(m => m.Name == "TryParseEnum");

        public static MethodInfo GetTryParseMethodFor(Type targetType)
        {
            return _tryParseMethod.MakeGenericMethod(targetType);
        }

        public static MethodInfo GetTryParseEnumMethodFor(Type targetEnumType)
        {
            return _tryParseEnumMethod.MakeGenericMethod(targetEnumType);
        }
    }
}