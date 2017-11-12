namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Extensions;
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

            var convertedValue = settings.ConvertTryParseCall(methodCall);
            var fallbackValue = GetFallbackValue(tryParseOrDefault);
            var nullString = default(string).ToConstantExpression();
            var sourceIsNotNull = Expression.NotEqual(methodCall.Arguments[0], nullString);
            var convertedOrFallback = Expression.Condition(sourceIsNotNull, convertedValue, fallbackValue);

            converted = assignment.Update(convertedOrFallback);
            return true;
        }

        private static Expression GetFallbackValue(ConditionalExpression tryParseOrDefault)
        {
            var defaultValue = tryParseOrDefault.IfFalse;

            if (defaultValue.NodeType != ExpressionType.Default)
            {
                return defaultValue;
            }

            return DefaultExpressionConverter.Convert((DefaultExpression)defaultValue);
        }
    }
}