namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData)
            => !mappingData.MapperData.SourceMember.IsEnumerable;

        protected override string GetNullMappingComment(Type targetType) => "No source enumerable available";

        protected override Expression GetNullMappingReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
            => Enumerable.Empty<Expression>();

        protected override Expression GetTypeTests(IObjectMappingData mappingData) => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            yield return mappingData.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(mappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.GetReturnValue();
    }
}