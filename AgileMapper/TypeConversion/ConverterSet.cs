namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Extensions;

    internal class ConverterSet
    {
        private readonly IValueConverter[] _converters;

        public ConverterSet()
        {
            var toStringConverter = new ToStringConverter();

            _converters = new IValueConverter[]
            {
                toStringConverter,
                //new ToDateTimeConverter(),
                new ToEnumConverter(toStringConverter),
                new ToNumericConverter<int>(toStringConverter),
                new DefaultTryParseConverter<Guid>(toStringConverter),
                new ToNumericConverter<decimal>(toStringConverter),
                new ToNumericConverter<long>(toStringConverter),
                new ToNumericConverter<short>(toStringConverter),
                new ToNumericConverter<byte>(toStringConverter)
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
        {
            if (sourceType.IsEnumerable() && targetType.IsEnumerable())
            {
                sourceType = sourceType.GetEnumerableElementType();
                targetType = targetType.GetEnumerableElementType();
            }

            if (targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (targetType.IsComplex() && sourceType.IsComplex())
            {
                return true;
            }

            return GetConverterFor(sourceType, targetType) != null;
        }

        private IValueConverter GetConverterFor(Type sourceType, Type targetType)
        {
            sourceType = sourceType.GetNonNullableUnderlyingTypeIfAppropriate();
            targetType = targetType.GetNonNullableUnderlyingTypeIfAppropriate();

            return _converters.FirstOrDefault(c =>
                c.IsFor(targetType) && c.CanConvert(sourceType));
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == targetType)
            {
                return sourceValue;
            }

            if (targetType.IsAssignableFrom(sourceValue.Type))
            {
                return sourceValue.GetConversionTo(targetType);
            }

            if (!targetType.IsSimple())
            {
                return sourceValue;
            }

            var converter = GetConverterFor(sourceValue.Type, targetType);

            return converter.GetConversion(sourceValue, targetType);
        }
    }
}