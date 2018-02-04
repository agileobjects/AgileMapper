namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Converters;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using Settings;

    internal class QueryProjectionModifier : ExpressionVisitor, IQueryProjectionModifier
    {
        private QueryProjectionModifier(IObjectMappingData mappingData)
        {
            MapperData = mappingData.MapperData;
            Settings = mappingData.GetQueryProviderSettings();
        }

        public IQueryProviderSettings Settings { get; }

        public IMemberMapperData MapperData { get; }

        public static Expression Modify(Expression queryProjection, IObjectMappingData mappingData)
            => new QueryProjectionModifier(mappingData).Modify(queryProjection);

        public Expression Modify(Expression queryProjection)
            => VisitAndConvert(queryProjection, "Modify");

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (ComplexTypeToNullComparisonConverter.TryConvert(binary, this, out var converted))
            {
                return converted;
            }

            return base.VisitBinary(binary);
        }

        protected override Expression VisitConditional(ConditionalExpression conditional)
        {
            if (ComplexTypeConditionalConverter.TryConvert(conditional, this, out var converted))
            {
                return Modify(converted);
            }

            return base.VisitConditional(conditional);
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant.Value is LambdaExpression lambda)
            {
                return Modify(lambda).ToConstantExpression(constant.Type);
            }

            return base.VisitConstant(constant);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultValueConstantExpressionFactory.CreateFor(defaultExpression);

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (TryParseAssignmentConverter.TryConvert(assignment, this, out var converted))
            {
                return converted;
            }

            if (NestedProjectionAssignmentConverter.TryConvert(assignment, this, out converted))
            {
                return converted;
            }

            return base.VisitMemberAssignment(assignment);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (ToStringConverter.TryConvert(methodCall, this, out var converted))
            {
                return converted;
            }

            if (GetValueOrDefaultConverter.TryConvert(methodCall, this, out converted))
            {
                return converted;
            }

            if (StringEqualsIgnoreCaseConverter.TryConvert(methodCall, this, out converted))
            {
                return converted;
            }

            return base.VisitMethodCall(methodCall);
        }
    }
}