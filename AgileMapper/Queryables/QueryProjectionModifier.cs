namespace AgileObjects.AgileMapper.Queryables
{
    using Converters;
    using Members;
    using ObjectPopulation;
    using Settings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

            if (StringConcatConverter.TryConvert(binary, this, out converted))
            {
                return converted;
            }

            return base.VisitBinary(binary);
        }

        protected override Expression VisitBlock(BlockExpression block)
        {
            if (DerivedTypeMappingConverter.TryConvert(block, this, out var converted))
            {
                return converted;
            }

            return base.VisitBlock(block);
        }

        protected override Expression VisitConditional(ConditionalExpression conditional)
        {
            if (ComplexTypeConditionalConverter.TryConvert(conditional, this, out var converted))
            {
                return Modify(converted);
            }

            if (StringToEnumConversionConverter.TryConvert(conditional, this, out converted))
            {
                return converted;
            }

            return base.VisitConditional(conditional);
        }

        protected override Expression VisitDefault(DefaultExpression defaultExpression)
            => DefaultValueConstantExpressionFactory.CreateFor(defaultExpression);

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (NullableConversionConverter.TryConvert(assignment, this, out var converted))
            {
                return converted;
            }

            if (TryParseAssignmentConverter.TryConvert(assignment, this, out converted))
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