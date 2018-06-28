namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ObjectPopulation;

    internal struct ConfiguredRootSourceDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            if (!context.MapperData.Parent.IsRoot ||
                !context.MapperData.MapperContext.UserConfigurations.HasDataSourceFactoriesForRootTarget)
            {
                yield break;
            }

            var rootMapperData = context.MapperData.Parent;

            var configuredRootTargetDataSources = context
                .MapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(rootMapperData);

            if (configuredRootTargetDataSources.None())
            {
                yield break;
            }

            var nullEnumerableIndex = typeof(int?).ToDefaultExpression();

            foreach (var configuredRootTargetDataSource in configuredRootTargetDataSources)
            {
                var mappingData = context
                    .ChildMappingData
                    .Parent.WithSource(configuredRootTargetDataSource.SourceMember);

                var mappingValues = new MappingValues(
                    configuredRootTargetDataSource.Value,
                    context.MapperData.TargetObject,
                    nullEnumerableIndex);

                var inlineMappingBlock = MappingFactory.GetInlineMappingBlock(
                    mappingData,
                    mappingValues,
                    MappingDataCreationFactory.ForDerivedType(mappingData.MapperData));

                if (!inlineMappingBlock.TryGetMappingBody(out var mappingBody))
                {
                    // TODO: Null mappings from a configured root source member
                }

                if (mappingBody.NodeType == ExpressionType.Block)
                {
                    IList<Expression> mappingExpressions = ((BlockExpression)mappingBody).Expressions;

                    Expression localVariable;

                    if (mappingExpressions.TryGetVariableAssignment(out var localVariableAssignment))
                    {
                        localVariable = localVariableAssignment.Left;
                        mappingExpressions = mappingExpressions.ToList();
                        mappingExpressions.Remove(localVariableAssignment);
                    }
                    else
                    {
                        localVariable = null;
                    }

                    // TODO: Test coverage for multiple-expression member mappings
                    mappingBody = mappingExpressions.HasOne()
                        ? mappingExpressions[0]
                        : Expression.Block(mappingExpressions);

                    if (localVariable != null)
                    {
                        mappingBody = mappingBody.Replace(
                            localVariable,
                            context.ChildMappingData.MapperData.TargetInstance);
                    }
                }

                // TODO: Test coverage for conditional configured source members
                yield return new ConfiguredDataSource(
                    configuredRootTargetDataSource.SourceMember,
                    configuredRootTargetDataSource.Condition,
                    mappingBody,
                    context.MapperData);
            }
        }
    }
}