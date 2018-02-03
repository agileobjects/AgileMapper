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
        {
            return new QueryProjectionModifier(mappingData).Modify(queryProjection);
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

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (ComplexTypeToNullComparisonConverter.TryConvert(binary, _settings, _mapperData, out var converted))
            {
                return converted;
            }

            return base.VisitBinary(binary);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultExpressionConverter.Convert(defaultExpression);
    }
}