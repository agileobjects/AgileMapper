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
                    return SourceCodeFactory.SourceCode(sc =>
                    {
                        foreach (var mappingFunction in mappingPlan)
                        {
                            var sourceTypeName = mappingFunction.SourceType.GetVariableNameInPascalCase();
                            var targetTypeName = mappingFunction.TargetType.GetVariableNameInPascalCase();
                            var className = $"{sourceTypeName}_To_{targetTypeName}_Mapper";

                            sc.WithClass(className, cls => cls
                                .WithMethod(
                                    "Map",
                                    mappingFunction.Summary,
                                    mappingFunction.Mapping));
                        }

                        return sc;
                    });
                })
                .ToList();
        }
    }
}
