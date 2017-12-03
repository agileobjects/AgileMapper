﻿namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class ConverterSet
    {
        private readonly List<IValueConverter> _converters;

        public ConverterSet()
        {
            var toStringConverter = new ToStringConverter();

            _converters = new List<IValueConverter>
            {
                toStringConverter,
                new ToNumericConverter<int>(toStringConverter),
                new ToBoolConverter(toStringConverter),
                new ToEnumConverter(toStringConverter),
                new DefaultTryParseConverter<DateTime>(toStringConverter),
                new DefaultTryParseConverter<Guid>(toStringConverter),
                new ToNumericConverter<decimal>(toStringConverter),
                new ToNumericConverter<double>(toStringConverter),
                new ToNumericConverter<long>(toStringConverter),
                new ToCharacterConverter(toStringConverter),
                new ToNumericConverter<short>(toStringConverter),
                new ToNumericConverter<byte>(toStringConverter),
                new FallbackNonSimpleTypeValueConverter()
            };
        }

        public void Add(IValueConverter converter)
        {
            _converters.Insert(0, converter);
        }

        public void ThrowIfUnconvertible(Type sourceType, Type targetType)
        {
            if (CanConvert(sourceType, targetType))
            {
                return;
            }

            var sourceTypeName = sourceType.GetFriendlyName();
            var targetTypeName = targetType.GetFriendlyName();

            throw new MappingConfigurationException(
                $"Unable to convert configured {sourceTypeName} to target type {targetTypeName}");
        }

        public bool CanConvert(Type sourceType, Type targetType)
            => sourceType.IsAssignableTo(targetType) || EnumerateConverters(sourceType, targetType).Any();

        private IEnumerable<IValueConverter> EnumerateConverters(Type sourceType, Type targetType)
        {
            sourceType = sourceType.GetNonNullableType();
            targetType = targetType.GetNonNullableType();

            foreach (var converter in _converters.Where(c => c.CanConvert(sourceType, targetType)))
            {
                yield return converter;

                if (!converter.IsConditional)
                {
                    yield break;
                }
            }
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == targetType)
            {
                return sourceValue;
            }

            if (sourceValue.Type.IsAssignableTo(targetType))
            {
                return targetType.IsNullableType() || (sourceValue.Type.IsSimple() && (targetType == typeof(object)))
                    ? sourceValue.GetConversionTo(targetType)
                    : sourceValue;
            }

            var converters = EnumerateConverters(sourceValue.Type, targetType).ToArray();

            var conversion = converters.ReverseChain(
                converter => converter.GetConversion(sourceValue, targetType),
                (conversionSoFar, converter) =>
                    converter.GetConversionOption(sourceValue, targetType, conversionSoFar));

            return conversion;
        }

        public void CloneTo(ConverterSet converterSet)
        {
            if (_converters.Count == converterSet._converters.Count)
            {
                return;
            }

            var numberOfCustomConverters = _converters.Count - converterSet._converters.Count;

            converterSet._converters.InsertRange(
                0,
                _converters.Take(numberOfCustomConverters));
        }
    }
}