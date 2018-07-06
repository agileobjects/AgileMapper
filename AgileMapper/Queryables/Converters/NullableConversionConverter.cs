namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class NullableConversionConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier,
            out MemberAssignment converted)
        {
            if (assignment.Expression.NodeType != ExpressionType.Convert)
            {
                converted = null;
                return false;
            }

            var conversion = (UnaryExpression)assignment.Expression;

            if (!conversion.Operand.Type.IsNullableType())
            {
                converted = null;
                return false;
            }

            var value = GetNullCheckedConversion(conversion.Operand, conversion);

            converted = assignment.Update(value);
            return true;
        }

        public static ConditionalExpression GetNullCheckedConversion(Expression nullableValue, Expression nonNullResult)
        {
            var nullValue = DefaultValueConstantExpressionFactory.CreateFor(nullableValue);
            var fallbackValue = DefaultValueConstantExpressionFactory.CreateFor(nonNullResult);

            var value = Expression.Condition(
                Expression.NotEqual(nullableValue, nullValue),
                nonNullResult,
                fallbackValue);

            return value;
        }
    }
}