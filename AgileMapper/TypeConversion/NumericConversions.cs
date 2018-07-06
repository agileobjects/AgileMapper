namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class NumericConversions
    {
        public static Expression GetModuloOneIsNotZeroCheck(Expression sourceValue)
            => GetModuloCheck(sourceValue, Expression.NotEqual);

        public static Expression GetModuloOneIsZeroCheck(Expression sourceValue)
            => GetModuloCheck(sourceValue, Expression.Equal);

        private static Expression GetModuloCheck(
            Expression sourceValue,
            Func<Expression, Expression, Expression> comparator)
        {
            var one = GetConstantValue(1, sourceValue);
            var sourceValueModuloOne = Expression.Modulo(sourceValue, one);
            var zero = GetConstantValue(0, sourceValue);
            var moduloOneEqualsZero = comparator(sourceValueModuloOne, zero);

            return moduloOneEqualsZero;
        }

        public static Expression GetConstantValue(int value, Expression sourceValue)
        {
            var sourceType = sourceValue.Type.GetNonNullableType();

            var constant = (sourceType == typeof(int))
                ? value.ToConstantExpression()
                : Expression.Constant(Convert.ChangeType(value, sourceType), sourceType);

            if (sourceType == sourceValue.Type)
            {
                return constant;
            }

            return constant.GetConversionTo(sourceValue.Type);
        }
    }
}