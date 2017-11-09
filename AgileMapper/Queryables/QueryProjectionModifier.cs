namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        public Expression Modify(Expression queryProjection)
        {
            return VisitAndConvert(queryProjection, "Modify");
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (TryParseAssignmentConverter.TryConvert(assignment, out var converted))
            {
                return converted;
            }

            return base.VisitMemberAssignment(assignment);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (StringEqualsIgnoreCaseConverter.TryConvert(methodCall, out var converted))
            {
                return converted;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultExpressionConverter.Convert(defaultExpression);
    }
}