namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;

    public interface IValueConverter
    {
        bool IsFor(Type nonNullableTargetType);

        bool CanConvert(Type sourceType);

        Expression GetConversion(Expression sourceValue, Type targetType);
    }
}