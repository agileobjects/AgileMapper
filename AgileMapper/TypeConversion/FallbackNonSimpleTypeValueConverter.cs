namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using ReadableExpressions.Extensions;

    internal struct FallbackNonSimpleTypeValueConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            if (nonNullableTargetType.IsSimple())
            {
                return false;
            }

            if (nonNullableTargetType.IsDictionary())
            {
                return true;
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