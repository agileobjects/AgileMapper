namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override bool TargetTypeIsNotConstructable(IObjectMappingData mappingData) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
            => Enumerable.Empty<Expression>();

        protected override Expression GetTypeTests(IObjectMappingData mappingData) => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            yield return mappingData.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(mappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
        {
            return mapperData.SourceMember.IsEnumerable
                ? mapperData.EnumerablePopulationBuilder.GetReturnValue()
                : mapperData.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}