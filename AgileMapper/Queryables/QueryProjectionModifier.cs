namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using Converters;
    using Extensions.Internal;
    using ObjectPopulation;
    using Settings;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        private readonly ObjectMapperData _mapperData;
        private readonly IQueryProviderSettings _settings;

        private QueryProjectionModifier(IObjectMappingData mappingData)
        {
            _mapperData = mappingData.MapperData;
            _settings = mappingData.GetQueryProviderSettings();
        }

        public static Expression Modify(Expression queryProjection, IObjectMappingData mappingData)
            => new QueryProjectionModifier(mappingData).Modify(queryProjection);

        private Expression Modify(Expression queryProjection)
            => VisitAndConvert(queryProjection, "Modify");

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (ComplexTypeToNullComparisonConverter.TryConvert(binary, _settings, _mapperData, out var converted))
            {
                return converted;
            }

            return base.VisitBinary(binary);
        }

        protected override Expression VisitConditional(ConditionalExpression conditional)
        {
            if (ComplexTypeConditionalConverter.TryConvert(conditional, _settings, _mapperData, out var converted))
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
            => NullConstantExpressionFactory.CreateFor(defaultExpression);

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
    }
}