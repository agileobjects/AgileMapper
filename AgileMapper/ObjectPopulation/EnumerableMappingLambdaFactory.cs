namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        protected override bool IsNotConstructable(IObjectMappingData mappingData) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(
            GotoExpression returnNull, 
            ObjectMapperData mapperData)
            => Enumerable.Empty<Expression>();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            yield return mappingData.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(mappingData.MapperData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
        {
            return mapperData.SourceMember.IsEnumerable
                ? mapperData.EnumerablePopulationBuilder.GetReturnValue()
                : mapperData.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}