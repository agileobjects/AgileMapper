namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        public static ObjectMappingLambdaFactoryBase Instance = new EnumerableMappingLambdaFactory();

        protected override bool IsNotConstructable(IObjectMappingContext omc) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
            => Enumerable.Empty<Expression>();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            yield return omc.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(omc);
        }

        protected override Expression GetReturnValue(IObjectMappingContext omc)
        {
            return omc.SourceMember.IsEnumerable
                ? omc.EnumerablePopulationBuilder.GetReturnValue()
                : omc.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}