namespace AgileObjects.AgileMapper.Queryables
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;
    using Settings;

    internal class QueryProjectionExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new QueryProjectionExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            return mapperData.TargetMember.IsEnumerable && mapperData.SourceType.IsQueryable();
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var queryable = mappingData.GetSource<IQueryable>();
            var providerSettings = QueryProviderSettings.For(queryable);

            var queryProjection = mapperData
                .EnumerablePopulationBuilder
                .GetSourceItemsProjection(
                    mapperData.SourceObject,
                    sourceParameter => MappingFactory.GetElementMapping(
                        sourceParameter,
                        mapperData.TargetMember.ElementType.ToDefaultExpression(),
                        mappingData));

            queryProjection = QueryProjectionModifier.Modify(queryProjection, providerSettings);

            yield return queryProjection;
        }
    }
}
