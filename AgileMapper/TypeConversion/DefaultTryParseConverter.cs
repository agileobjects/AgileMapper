namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;

    internal class DefaultTryParseConverter<T> : TryParseConverterBase
    {
        public DefaultTryParseConverter(ToStringConverter toStringConverter)
            : base(toStringConverter, typeof(T))
        {
        }

        protected override bool CanConvert(Type nonNullableSourceType)
            => (nonNullableSourceType == typeof(string)) || base.CanConvert(nonNullableSourceType);
    }
}