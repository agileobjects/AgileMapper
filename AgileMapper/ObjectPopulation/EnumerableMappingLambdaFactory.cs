namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class EnumerableMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        public static ObjectMappingLambdaFactoryBase Instance = new EnumerableMappingLambdaFactory();

        protected override bool IsNotConstructable(IObjectMapperCreationData data) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData data)
            => Enumerable.Empty<Expression>();

        protected override ObjectPopulation GetObjectPopulation(IObjectMapperCreationData data)
            => new ObjectPopulation(data.RuleSet.EnumerablePopulationStrategy.GetPopulation(data.MapperData));

        protected override Expression GetReturnValue(ObjectMapperData omc)
        {
            return omc.SourceMember.IsEnumerable
                ? omc.EnumerablePopulationBuilder.GetReturnValue()
                : omc.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}