namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

            var convertedValue = modifier.Settings.ConvertTryParseCall(methodCall, tryParseOrDefault.IfFalse);

            converted = assignment.Update(convertedValue);
            return true;
        }
    }
}