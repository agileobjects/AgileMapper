namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;

    internal class ConfiguredValueConverter : ValueConverterBase
    {
        private readonly Type _sourceType;
        private readonly Expression _value;

        public ConfiguredValueConverter(
            Type sourceType,
            Expression value,
            Func<Expression, Type, Expression> conditionFactory = null)
            : base(conditionFactory)
        {
            _sourceType = sourceType;
            _value = value;
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => (nonNullableSourceType == _sourceType) && (nonNullableTargetType == _value.Type);

        public override Expression GetConversion(Expression sourceValue, Type targetType) => _value;
    }
}