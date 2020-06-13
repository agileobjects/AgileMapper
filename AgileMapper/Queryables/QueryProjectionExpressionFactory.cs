namespace AgileObjects.AgileMapper.Queryables
{
    using Extensions.Internal;
    using ObjectPopulation;

    internal class QueryProjectionExpressionFactory : MappingExpressionFactoryBase
    {
        protected override void AddObjectPopulation(MappingCreationContext context)
        {
            var mapperData = context.MapperData;

            var queryProjection = mapperData
                .EnumerablePopulationBuilder
                .GetSourceItemsProjection(
                    mapperData.SourceObject,
                    sourceParameter => MappingFactory.GetElementMapping(
                        sourceParameter,
                        mapperData.TargetMember.ElementType.ToDefaultExpression(),
                        context.MappingData));

            queryProjection = QueryProjectionModifier.Modify(queryProjection, context.MappingData);

            context.MappingExpressions.Add(queryProjection);
        }
    }
}
