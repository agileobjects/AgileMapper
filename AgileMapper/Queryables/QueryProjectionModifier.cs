namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using System.Linq.Expressions;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        private readonly IQueryable _queryable;

        private QueryProjectionModifier(IQueryable queryable)
        {
            _queryable = queryable;
        }

        public static Expression Modify(Expression queryProjection, IQueryable queryable)
        {
            return new QueryProjectionModifier(queryable).Modify(queryProjection);
        }

        private Expression Modify(Expression queryProjection)
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
            if (StringEqualsIgnoreCaseConverter.TryConvert(methodCall, _queryable, out var converted))
            {
                return converted;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultExpressionConverter.Convert(defaultExpression);
    }
}