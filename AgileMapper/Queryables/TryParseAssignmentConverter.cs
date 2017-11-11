namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;
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
            // ReSharper disable once PossibleNullReferenceException
            // Attempt to use Convert.ToInt32 - irretrievably unsupported in non-EDMX EF5 and EF6, 
            // but it at least gives a decent error message:
            var convertMethodName = "To" + tryParseCall.Method.DeclaringType.Name;

            var convertMethod = typeof(Convert)
                .GetPublicStaticMethods(convertMethodName)
                .First(m => m.GetParameters().HasOne() && (m.GetParameters()[0].ParameterType == typeof(string)));

            var convertCall = Expression.Call(convertMethod, tryParseCall.Arguments.First());

            return convertCall;
        }
    }
}