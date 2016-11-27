namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class FallbackNonSimpleTypeValueConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            if (nonNullableTargetType.IsSimple())
            {
                return false;
            }

            if (nonNullableTargetType.IsEnumerable())
            {
                return nonNullableSourceType.IsEnumerable();
            }

            return true;
        }

        public Expression GetConversion(Expression sourceValue, Type targetType) => sourceValue;
    }
}