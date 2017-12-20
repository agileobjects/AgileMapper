namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Settings;

    internal static class TryParseAssignmentConverter
    {
        public static bool TryConvert(
            MemberAssignment assignment,
            IQueryProviderSettings settings,
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

            if (tryParseOrDefault.Test.NodeType != ExpressionType.Call)
            {
                converted = null;
                return false;
            }

            var methodCall = (MethodCallExpression)tryParseOrDefault.Test;

            if (!methodCall.Method.IsStatic || (methodCall.Method.Name != "TryParse"))
            {
                converted = null;
                return false;
            }

            var convertedValue = settings.ConvertTryParseCall(methodCall, tryParseOrDefault.IfFalse);

            converted = assignment.Update(convertedValue);
            return true;
        }
    }
}