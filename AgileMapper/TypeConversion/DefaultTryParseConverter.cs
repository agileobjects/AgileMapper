namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;

    internal class DefaultTryParseConverter<T> : TryParseConverterBase
    {
        public DefaultTryParseConverter(ToStringConverter toStringConverter)
            : base(toStringConverter, typeof(T))
        {
        }

        public override bool CanConvert(Type nonNullableSourceType)
        {
            return (nonNullableSourceType == typeof(string)) || base.CanConvert(nonNullableSourceType);
        }
    }
}