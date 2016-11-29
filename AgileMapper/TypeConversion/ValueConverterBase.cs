namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;

    internal abstract class ValueConverterBase : IValueConverter
    {
        private readonly Func<Expression, Type, Expression> _conditionFactory;

        protected ValueConverterBase(Func<Expression, Type, Expression> conditionFactory = null)
        {
            _conditionFactory = conditionFactory;
            IsConditional = conditionFactory != null;
        }

        public abstract bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType);

        public bool IsConditional { get; }

        public abstract Expression GetConversion(Expression sourceValue, Type targetType);

        public Expression GetConversionOption(Expression sourceValue, Type targetType, Expression conversionSoFar)
        {
            var condition = _conditionFactory.Invoke(sourceValue, targetType);
            var conversion = GetConversion(sourceValue, targetType);

            return Expression.Condition(condition, conversion, conversionSoFar);
        }
    }
}