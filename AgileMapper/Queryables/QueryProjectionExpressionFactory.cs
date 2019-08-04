﻿namespace AgileObjects.AgileMapper.Queryables
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using ObjectPopulation;

    internal class QueryProjectionExpressionFactory : MappingExpressionFactoryBase
    {
        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
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

            yield return queryProjection;
        }
    }
}
