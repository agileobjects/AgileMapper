namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal class DefaultMemberPopulationFactory : MemberPopulationFactoryBase
    {
        public static readonly IMemberPopulationFactory Instance = new DefaultMemberPopulationFactory();

        protected override Expression GetPopulationGuard(IMemberPopulationContext context)
            => context.PopulateCondition;

        protected override Expression GetGuardedBindingValue(Expression bindingValue, Expression populationGuard)
        {
            if (populationGuard == null)
            {
                return bindingValue;
            }

            return Expression.Condition(
                populationGuard,
                bindingValue,
                bindingValue.Type.ToDefaultExpression());
        }

        public override Expression GetGuardedPopulation(
            Expression population,
            Expression populationGuard,
            bool useSingleExpression)
        {
            return useSingleExpression
                ? population
                : base.GetGuardedPopulation(population, populationGuard, false);
        }
    }
}