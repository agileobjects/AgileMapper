namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;

    internal class DefaultTryParseConverter<T> : TryParseConverterBase
    {
        public DefaultTryParseConverter()
            : base(typeof(T))
        {
        }

        public override bool CanConvert(Type nonNullableSourceType)
        {
            return base.CanConvert(nonNullableSourceType) || (nonNullableSourceType == typeof(string));
        }
    }
}