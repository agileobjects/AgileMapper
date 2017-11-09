namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal static class TryParseAssignmentConverter
    {
        public static bool TryConvert(MemberAssignment assignment, out MemberAssignment converted)
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

            if (methodCall.Method.IsStatic && (methodCall.Method.Name == "TryParse"))
            {
                converted = assignment.Update(GetConvertCall(methodCall));
                return true;
            }

            converted = null;
            return false;
        }

        private static MethodCallExpression GetConvertCall(MethodCallExpression tryParseCall)
        {
            var parseMethod = tryParseCall
                .Method
                .DeclaringType
                .GetPublicStaticMethod("Parse");

            var parseCall = Expression.Call(parseMethod, tryParseCall.Arguments.First());

            return parseCall;
        }
    }
}