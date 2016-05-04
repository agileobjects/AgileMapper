namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

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

        protected override bool IsCoercible(Expression sourceValue)
        {
            return _coercibleNumericTypes.Contains(sourceValue.Type);
        }
    }
}