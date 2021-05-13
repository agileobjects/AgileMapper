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

    internal struct OperatorConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => GetOperatorOrNull(nonNullableSourceType, nonNullableTargetType) != null;

        public static MethodInfo GetOperatorOrNull(
            Type nonNullableSourceType,
            Type nonNullableTargetType)
        {
            if (!nonNullableSourceType.IsPrimitive())
            {
                var operatorMethod = GetOperatorOrNull(
                    nonNullableSourceType,
                    o => o.ReturnType == nonNullableTargetType);

                if (operatorMethod != null)
                {
                    return operatorMethod;
                }
            }

            if (nonNullableTargetType.IsPrimitive())
            {
                return null;
            }

            return GetOperatorOrNull(
                nonNullableTargetType,
                o => o.GetParameters()[0].ParameterType == nonNullableSourceType);
        }

        private static MethodInfo GetOperatorOrNull(Type subjectType, Func<MethodInfo, bool> matcher)
            => subjectType.GetOperators().FirstOrDefault(matcher.Invoke);

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();
            var nonNullableTargetType = targetType.GetNonNullableType();
            var operatorMethod = GetOperatorOrNull(nonNullableSourceType, nonNullableTargetType);

            return Expression.Convert(sourceValue, nonNullableTargetType, operatorMethod);
        }
    }
}