namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConverterSet
    {
        private readonly List<IValueConverter> _converters;

        public ConverterSet(UserConfigurationSet userConfigurations)
        {
            _converters = new List<IValueConverter>
            {
                default(ToStringConverter),
                ToNumericConverter<int>.Instance,
                default(ToBoolConverter),
                new ToEnumConverter(userConfigurations),
                TryParseConverter<DateTime>.Instance,
#if NET35
                ToGuidConverter.Instance,
#else
                TryParseConverter<Guid>.Instance,
#endif
                ToNumericConverter<decimal>.Instance,
                ToNumericConverter<double>.Instance,
                ToNumericConverter<long>.Instance,
                default(ToCharacterConverter),
                ToNumericConverter<short>.Instance,
                ToNumericConverter<byte>.Instance,
                default(FallbackNonSimpleTypeValueConverter)
            };
        }

        public void Add(IValueConverter converter) => _converters.Insert(0, converter);

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
            => sourceType.IsAssignableTo(targetType) || (GetConverterOrNull(sourceType, targetType) != null);

        private IValueConverter GetConverterOrNull(Type sourceType, Type targetType)
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

            if (sourceValue.Type.IsAssignableTo(targetType))
            {
                return ConvertSourceValueToTargetType(sourceValue, targetType)
                    ? sourceValue.GetConversionTo(targetType)
                    : sourceValue;
            }

            var converter = GetConverterOrNull(sourceValue.Type, targetType);
            var conversion = converter.GetConversion(sourceValue, targetType);

            return conversion;
        }

        private static bool ConvertSourceValueToTargetType(Expression sourceValue, Type targetType)
        {
            if (targetType.IsNullableType())
            {
                return true;
            }

            if (!sourceValue.Type.IsValueType() && !sourceValue.Type.IsSimple())
            {
                return false;
            }

            return (targetType == typeof(object)) || (targetType == typeof(ValueType));
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