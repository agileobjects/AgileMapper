namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;

    public interface IValueConverter
    {
        bool IsFor(Type nonNullableTargetType);

        bool CanConvert(Type nonNullableSourceType);

        Expression GetConversion(Expression sourceValue, Type targetType);
    }
}