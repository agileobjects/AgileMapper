namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BuildableExpressions;
    using BuildableExpressions.SourceCode;
    using Configuration;

    /// <summary>
    /// An <see cref="ISourceCodeExpressionBuilder"/> which generates source code for configured
    /// mappers.
    /// </summary>
    public class MapperBuilder : ISourceCodeExpressionBuilder
    {
        /// <inheritdoc />
        public IEnumerable<SourceCodeExpression> Build(IExpressionBuildContext context)
        {
            using var mapper = Mapper.CreateNew();

            mapper.WhenMapping
                .UseConfigurations.From(context.ProjectAssemblies);

            var mapperSourceCode = mapper
                .GetPlanSourceCodeInCache(context.RootNamespace)
                .ToList();

            if (mapperSourceCode.Any())
            {
                return mapperSourceCode;
            }

            context.Log(
                $"no {nameof(BuildableMapperConfiguration)}-derived " +
                 "Mapper configurations found");

            return Array.Empty<SourceCodeExpression>();
        }
    }
}