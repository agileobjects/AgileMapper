namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ReadableExpressions.Extensions;

    internal static class TryParseAssignmentConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProjectionModifier modifier,
            out MemberAssignment converted)
        {
            if (assignment.Expression.NodeType != ExpressionType.Block)
            {
                converted = null;
                return false;
            }

            var valueBlock = (BlockExpression)assignment.Expression;
            var finalExpression = valueBlock.Expressions.Last();

            if (finalExpression.NodeType != ExpressionType.Conditional)
            {
                converted = null;
                return false;
            }

            var tryParseOrDefault = (ConditionalExpression)finalExpression;

            if ((tryParseOrDefault.Test.NodeType != ExpressionType.Call) ||
                (tryParseOrDefault.IfTrue.NodeType != ExpressionType.Parameter))
            {
                converted = null;
                return false;
            }

            var methodCall = (MethodCallExpression)tryParseOrDefault.Test;

            if (!methodCall.Method.IsStatic || (methodCall.Method.Name != nameof(int.TryParse)))
            {
                converted = null;
                return false;
            }

            var convertedValue = GetConvertedValue(modifier, methodCall, tryParseOrDefault);

            converted = assignment.Update(convertedValue);
            return true;
        }

        private static Expression GetConvertedValue(
            IQueryProjectionModifier modifier,
            MethodCallExpression methodCall,
            ConditionalExpression tryParseOrDefault)
        {
            var convertedValue = modifier.Settings.ConvertTryParseCall(methodCall, tryParseOrDefault.IfFalse);

            return AdjustForNullableSourceIfAppropriate(methodCall, convertedValue, tryParseOrDefault);
        }

        private static Expression AdjustForNullableSourceIfAppropriate(
            MethodCallExpression methodCall,
            Expression convertedValue,
            ConditionalExpression tryParseOrDefault)
        {
            var parsedValue = GetParsedValue(methodCall);

            if (!parsedValue.Type.IsNullableType())
            {
                return convertedValue;
            }

            var nullableDefault = DefaultValueConstantExpressionFactory.CreateFor(parsedValue);

            convertedValue = Expression.Condition(
                Expression.NotEqual(parsedValue, nullableDefault),
                convertedValue,
                tryParseOrDefault.IfFalse);

            return convertedValue;
        }

        private static Expression GetParsedValue(MethodCallExpression methodCall)
        {
            var parsedValue = methodCall.Arguments.First();

            if (parsedValue.NodeType != ExpressionType.Call)
            {
                return parsedValue;
            }

            return ((MethodCallExpression)parsedValue).Object;
        }
    }
}