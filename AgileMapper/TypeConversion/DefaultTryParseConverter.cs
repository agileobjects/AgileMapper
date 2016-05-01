namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;

    internal class DefaultTryParseConverter<T> : TryParseConverterBase
    {
        public DefaultTryParseConverter()
            : base(typeof(T))
        {
        }

        public override bool CanConvert(Type sourceType)
        {
            return base.CanConvert(sourceType) ||
                   (sourceType == typeof(string));
        }
    }
}