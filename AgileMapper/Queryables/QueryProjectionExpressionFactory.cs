namespace AgileObjects.AgileMapper.Queryables
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class QueryProjectionExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new QueryProjectionExpressionFactory();

        #region Cached Items

        private static readonly MethodInfo _queryableSelectMethod = typeof(Queryable)
            .GetPublicStaticMethods()
            .First(m =>
                (m.Name == "Select") &&
                (m.GetParameters().Last().ParameterType.GetGenericArguments().First().GetGenericArguments().Length == 2));

        #endregion

        public override bool IsFor(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            return mapperData.TargetMember.IsEnumerable &&
                   mapperData.SourceType.IsGenericType() &&
                   mapperData.SourceType.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var queryProjection = mapperData
                .EnumerablePopulationBuilder
                .GetSourceItemsProjection(
                    mapperData.SourceObject,
                    _queryableSelectMethod,
                    sourceParameter => MappingFactory.GetElementMapping(
                        sourceParameter,
                        mapperData.TargetMember.ElementType.ToDefaultExpression(),
                        mappingData));

            yield return queryProjection;
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
        {
            throw new System.NotImplementedException();
        }
    }
}
