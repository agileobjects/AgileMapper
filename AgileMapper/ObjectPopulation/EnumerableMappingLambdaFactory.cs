namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class EnumerableMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        public static ObjectMappingLambdaFactoryBase Instance = new EnumerableMappingLambdaFactory();

        protected override bool IsNotConstructable(ObjectMapperData data) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData data)
            => Enumerable.Empty<Expression>();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMapperCreationData data)
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