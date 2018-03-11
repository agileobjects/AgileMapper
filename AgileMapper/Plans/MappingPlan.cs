namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using Validation;
    using static System.Linq.Expressions.ExpressionType;

    /// <summary>
    /// Contains details of the mapping plan for a mapping between a particular source and target type,
    /// for a particular mapping type (create new, merge, overwrite).
    /// </summary>
    public class MappingPlan
    {
        private readonly ObjectMapperData _mapperData;
        private readonly LambdaExpression _mappingLambda;

        internal MappingPlan(IObjectMapper cachedMapper)
        {
            _mapperData = cachedMapper.MapperData;
            _mappingLambda = cachedMapper.MappingLambda;
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to its string representation.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="MappingPlan"/> to convert.</param>
        /// <returns>The string representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator string(MappingPlan mappingPlan)
            => MappingPlanConverter.GetPlanFor(mappingPlan);

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlan"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlan"/>.</returns>
        public override string ToString() => this;

        #region Helper Class

        private class MappingPlanConverter : ExpressionVisitor
        {
            private readonly List<Expression> _recursionFuncAssignments;

            private MappingPlanConverter()
            {
                _recursionFuncAssignments = new List<Expression>();
            }

            public static string GetPlanFor(MappingPlan mappingPlan)
            {
                var mappingLambda = mappingPlan._mappingLambda;
                var mapperData = mappingPlan._mapperData;

                var lambdaWithEnumMismatches = EnumMappingMismatchFinder.Process(mappingLambda, mapperData);

                var converter = new MappingPlanConverter();

                var convertedLambda = converter.VisitAndConvert(lambdaWithEnumMismatches, nameof(MappingPlanConverter));

                var planBuilder = new StringBuilder();

                planBuilder.AppendLine(GetMapperHeader(mappingLambda.Parameters[0].Type, mapperData));
                planBuilder.AppendLine(convertedLambda.ToReadableString());

                foreach (var funcAssignment in converter._recursionFuncAssignments)
                {
                    planBuilder.AppendLine(GetMapperHeader(funcAssignment.Type, mapperData, forRecursionMapper: true));
                    planBuilder.AppendLine(funcAssignment.ToReadableString());
                }

                return planBuilder.ToString();
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                if ((binary.NodeType != Assign) ||
                    (binary.Left.NodeType != Parameter) ||
                    !binary.Left.Type.IsClosedTypeOf(typeof(MapperFunc<,>)))
                {
                    return base.VisitBinary(binary);
                }

                if (binary.Right.NodeType != Default)
                {
                    _recursionFuncAssignments.Add(binary);
                }

                return Constants.EmptyExpression;
            }

            private static string GetMapperHeader(
                Type mapperFuncType,
                IBasicMapperData mapperData,
                bool forRecursionMapper = false)
            {
                var mappingTypes = mapperFuncType.GetGenericTypeArguments();

                var planNote = forRecursionMapper
                    ? "Recursion Mapper"
                    : "Rule Set: " + mapperData.RuleSet.Name;

                return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {mappingTypes[0].GetFriendlyName()} -> {mappingTypes[1].GetFriendlyName()}
// {planNote}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
".TrimStart();
            }
        }

        #endregion
    }
}
