namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;

    internal abstract class MemberPopulationFactoryBase : IMemberPopulationFactory
    {
        public Expression GetPopulation(IMemberPopulationContext context)
        {
            if (!context.IsSuccessful)
            {
                return context.DataSources.ValueExpression;
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
                population = GetPopulationWithVariables(population, context.DataSources.Variables);
            }

            return GetGuardedPopulation(population, populationGuard, useSingleExpression);
        }

        private static Expression GetPopulationWithVariables(Expression population, IList<ParameterExpression> variables)
        {
            if (population.NodeType != ExpressionType.Block)
            {
                return Expression.Block(variables, population);
            }

            var populationBlock = (BlockExpression)population;

            if (populationBlock.Variables.Any())
            {
                variables = variables.Append(populationBlock.Variables);
            }

            return populationBlock.Update(variables, populationBlock.Expressions);
        }

        protected abstract Expression GetPopulationGuard(IMemberPopulationContext context);

        private Expression GetBinding(IMemberPopulationContext context, Expression populationGuard)
        {
            var bindingValue = context.DataSources.ValueExpression;
            var guardedBindingValue = GetGuardedBindingValue(bindingValue, populationGuard);
            var binding = context.MapperData.GetTargetMemberPopulation(guardedBindingValue);

            return binding;
        }

        protected abstract Expression GetGuardedBindingValue(Expression bindingValue, Expression populationGuard);

        private static Expression GetReadOnlyMemberPopulation(IMemberPopulationContext context)
        {
            var dataSourcesValue = context.DataSources.ValueExpression;
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