namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Configuration;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ConverterSet
    {
        private readonly IValueConverter[] _converters;

        public ConverterSet()
        {
            var toStringConverter = new ToStringConverter();

            _converters = new IValueConverter[]
            {
                toStringConverter,
                new ToEnumConverter(toStringConverter),
                new ToNumericConverter<int>(toStringConverter),
                new DefaultTryParseConverter<DateTime>(toStringConverter),
                new DefaultTryParseConverter<Guid>(toStringConverter),
                new ToNumericConverter<decimal>(toStringConverter),
                new ToNumericConverter<double>(toStringConverter),
                new ToNumericConverter<long>(toStringConverter),
                new ToNumericConverter<short>(toStringConverter),
                new ToNumericConverter<byte>(toStringConverter),
                new FallbackNonSimpleTypeValueConverter()
            };
        }

        public void ThrowIfUnconvertible(Type sourceType, Type targetType)
        {
            if (!CanConvert(sourceType, targetType))
            {
                throw new MappingConfigurationException(
                    $"Unable to convert configured {sourceType.Name} to target type {targetType.Name}");
            }
        }

        public bool CanConvert(Type sourceType, Type targetType)
            => targetType.IsAssignableFrom(sourceType) || GetConverterFor(sourceType, targetType) != null;

        private IValueConverter GetConverterFor(Type sourceType, Type targetType)
        {
            sourceType = sourceType.GetNonNullableType();
            targetType = targetType.GetNonNullableType();

            return _converters.FirstOrDefault(c => c.CanConvert(sourceType, targetType));
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == targetType)
            {
                return sourceValue;
            }

            if (targetType.IsAssignableFrom(sourceValue.Type))
            {
                return targetType.IsNullableType() ? sourceValue.GetConversionTo(targetType) : sourceValue;
            }

            var converter = GetConverterFor(sourceValue.Type, targetType);

            return converter.GetConversion(sourceValue, targetType);
        }
    }
}