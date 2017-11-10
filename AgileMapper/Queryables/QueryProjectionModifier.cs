namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        private readonly QueryProviderSettings _settings;

        private QueryProjectionModifier(QueryProviderSettings settings)
        {
            _settings = settings;
        }

        public static Expression Modify(Expression queryProjection, QueryProviderSettings settings)
        {
            return new QueryProjectionModifier(settings).Modify(queryProjection);
        }

        private Expression Modify(Expression queryProjection)
        {
            return VisitAndConvert(queryProjection, "Modify");
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (TryParseAssignmentConverter.TryConvert(assignment, _settings, out var converted))
            {
                return converted;
            }

            return base.VisitMemberAssignment(assignment);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (StringEqualsIgnoreCaseConverter.TryConvert(methodCall, _settings, out var converted))
            {
                return converted;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultExpressionConverter.Convert(defaultExpression);
    }
}