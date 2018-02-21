namespace AgileObjects.AgileMapper.Queryables
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ObjectPopulation;

    internal class QueryProjectionExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new QueryProjectionExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            return mapperData.IsRoot &&
                   mapperData.TargetMember.IsEnumerable &&
                   mapperData.SourceType.IsQueryable();
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var queryProjection = mapperData
                .EnumerablePopulationBuilder
                .GetSourceItemsProjection(
                    mapperData.SourceObject,
                    sourceParameter => MappingFactory.GetElementMapping(
                        sourceParameter,
                        mapperData.TargetMember.ElementType.ToDefaultExpression(),
                        mappingData));

            queryProjection = QueryProjectionModifier.Modify(queryProjection, mappingData);

            yield return queryProjection;
        }
    }
}
