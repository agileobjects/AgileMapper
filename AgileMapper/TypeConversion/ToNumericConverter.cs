namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using Extensions.Internal;

    internal class ToNumericConverter<TNumeric> : ToNumericConverterBase
    {
        private static readonly Type[] _coercibleNumericTypes =
            typeof(TNumeric)
                .GetCoercibleNumericTypes()
                .ToArray();

        public ToNumericConverter(ToStringConverter toStringConverter)
            : base(toStringConverter, typeof(TNumeric))
        {
        }

        protected override bool IsCoercible(Type sourceType) => _coercibleNumericTypes.Contains(sourceType);
    }
}