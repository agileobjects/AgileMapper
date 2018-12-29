namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal struct OperatorConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => GetOperatorOrNull(nonNullableSourceType, nonNullableTargetType) != null;

        public static MethodInfo GetOperatorOrNull(
            Type nonNullableSourceType,
            Type nonNullableTargetType)
        {
            var operatorMethod = nonNullableSourceType
                .GetOperators()
                .FirstOrDefault(o => o.ReturnType == nonNullableTargetType);

            return operatorMethod;
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();
            var nonNullableTargetType = targetType.GetNonNullableType();
            var operatorMethod = GetOperatorOrNull(nonNullableSourceType, nonNullableTargetType);

            return Expression.Call(operatorMethod, sourceValue);
        }
    }
}