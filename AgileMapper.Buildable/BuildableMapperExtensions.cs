namespace AgileObjects.AgileMapper.Buildable
{
    using System.Collections.Generic;
    using System.Linq;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;
    using Plans;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides extension methods for building AgileMapper mapper source files.
    /// </summary>
    public static class BuildableMapperExtensions
    {
        /// <summary>
        /// Builds <see cref="SourceCodeExpression"/>s for the configured mappers in this
        /// <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">
        /// The <see cref="IMapper"/> for which to build <see cref="SourceCodeExpression"/>s.
        /// </param>
        /// <returns>
        /// A <see cref="SourceCodeExpression"/> for each mapper configured in this <paramref name="mapper"/>.
        /// </returns>
        public static IEnumerable<SourceCodeExpression> BuildSourceCode(
            this IMapper mapper)
        {
            return mapper
                .GetPlansInCache()
                .ToMapperSourceCodeExpressions();
        }

        /// <summary>
        /// Converts these <paramref name="mappingPlans"/> into <see cref="SourceCodeExpression"/>s.
        /// </summary>
        /// <param name="mappingPlans">The <see cref="MappingPlanSet"/> containing the mapping plans to convert.</param>
        /// <returns>A <see cref="SourceCodeExpression"/> for each of these <paramref name="mappingPlans"/>.</returns>
        public static IEnumerable<SourceCodeExpression> ToMapperSourceCodeExpressions(
            this IEnumerable<IMappingPlan> mappingPlans)
        {
            return mappingPlans
                .Select(mappingPlan =>
                {
                    var sourceCode = SourceCodeFactory.Default.CreateSourceCode();

                    var rootPlan = mappingPlan.Root;

                    var sourceTypeName = rootPlan.SourceType.GetVariableNameInPascalCase();
                    var targetTypeName = rootPlan.TargetType.GetVariableNameInPascalCase();
                    var className = $"{sourceTypeName}_To_{targetTypeName}_Mapper";

                    var mapperClass = sourceCode.AddClass(cls => cls.Named(className));

                    var coreMapMethod = mapperClass.AddMethod(rootPlan.Mapping, m => m
                        .WithSummary(rootPlan.Summary)
                        .WithVisibility(MemberVisibility.Private)
                        .Named("Map"));

                    var coreMapMethodCall = BuildableExpression.Call(coreMapMethod);

                    return sourceCode;
                })
                .ToList();
        }
    }
}
