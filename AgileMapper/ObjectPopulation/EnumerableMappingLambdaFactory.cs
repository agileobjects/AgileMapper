namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        protected override bool IsNotConstructable(IObjectMappingContextData data) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData data)
            => Enumerable.Empty<Expression>();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContextData data)
        {
            yield return data.RuleSet.EnumerablePopulationStrategy.GetPopulation(data.MapperData);
        }

        protected override Expression GetReturnValue(ObjectMapperData omc)
        {
            return omc.SourceMember.IsEnumerable
                ? omc.EnumerablePopulationBuilder.GetReturnValue()
                : omc.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}