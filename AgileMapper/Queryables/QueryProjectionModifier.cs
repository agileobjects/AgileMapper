namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Extensions;
    using Settings;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        private readonly IQueryProviderSettings _settings;

        private QueryProjectionModifier(IQueryProviderSettings settings)
        {
            _settings = settings;
        }

        public static Expression Modify(Expression queryProjection, IQueryProviderSettings settings)
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

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant.Value is LambdaExpression lambda)
            {
                return VisitAndConvert(lambda, "ModifyLambda").ToConstantExpression(constant.Type);
            }

            return base.VisitConstant(constant);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (ToStringConverter.TryConvert(methodCall, _settings, out var converted))
            {
                return converted;
            }

            if (GetValueOrDefaultConverter.TryConvert(methodCall, _settings, out converted))
            {
                return converted;
            }

            if (StringEqualsIgnoreCaseConverter.TryConvert(methodCall, _settings, out converted))
            {
                return converted;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultExpressionConverter.Convert(defaultExpression);

        protected override Expression VisitNewArray(NewArrayExpression newArray)
        {
            if (EmptyArrayConverter.TryConvert(newArray, _settings, out var converted))
            {
                return converted;
            }

            return base.VisitNewArray(newArray);
        }
    }
}