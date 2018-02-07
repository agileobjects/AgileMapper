namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal abstract class MemberPopulationFactoryBase : IMemberPopulationFactory
    {
        public Expression GetPopulation(IMemberPopulationContext context)
        {
            if (!context.IsSuccessful)
            {
                return context.DataSources.GetValueExpression();
            }

            var useSingleExpression = context.MapperData.UseMemberInitialisations();
            var populationGuard = GetPopulationGuard(context);

            var population = useSingleExpression
                ? GetBinding(context, populationGuard)
                : context.MapperData.TargetMember.IsReadOnly
                    ? GetReadOnlyMemberPopulation(context)
                    : context.DataSources.GetPopulationExpression();

            if (context.DataSources.Variables.Any())
            {
                population = Expression.Block(context.DataSources.Variables, population);
            }

            return GetGuardedPopulation(population, populationGuard, useSingleExpression);
        }

        protected abstract Expression GetPopulationGuard(IMemberPopulationContext context);

        private Expression GetBinding(IMemberPopulationContext context, Expression populationGuard)
        {
            var bindingValue = context.DataSources.GetValueExpression();
            var guardedBindingValue = GetGuardedBindingValue(bindingValue, populationGuard);
            var binding = context.MapperData.GetTargetMemberPopulation(guardedBindingValue);

            return binding;
        }

        protected abstract Expression GetGuardedBindingValue(Expression bindingValue, Expression populationGuard);

        private static Expression GetReadOnlyMemberPopulation(IMemberPopulationContext context)
        {
            var dataSourcesValue = context.DataSources.GetValueExpression();
            var targetMemberAccess = context.MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();

            return Expression.IfThen(targetMemberNotNull, dataSourcesValue);
        }

        public virtual Expression GetGuardedPopulation(
            Expression population,
            Expression populationGuard,
            bool useSingleExpression)
        {
            return (populationGuard != null)
                ? Expression.IfThen(populationGuard, population)
                : population;
        }
    }
}