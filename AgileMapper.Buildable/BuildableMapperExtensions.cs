namespace AgileObjects.AgileMapper.Buildable
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;

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
                .GetPlanExpressionsInCache()
                .ToMapperSourceCodeExpressions();
        }

        /// <summary>
        /// Converts these <paramref name="mappingExpressions"/> into <see cref="SourceCodeExpression"/>s.
        /// </summary>
        /// <param name="mappingExpressions">The mapping Expressions to convert.</param>
        /// <returns>A <see cref="SourceCodeExpression"/> for each of these <paramref name="mappingExpressions"/>.</returns>
        public static IEnumerable<SourceCodeExpression> ToMapperSourceCodeExpressions(
            this IEnumerable<Expression> mappingExpressions)
        {
            return mappingExpressions
                .Select(exp => SourceCodeFactory.SourceCode(exp))
                .ToList();
        }
    }
}
