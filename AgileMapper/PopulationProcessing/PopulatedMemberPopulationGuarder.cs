namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class PopulatedMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new PopulatedMemberPopulationGuarder();

        public void Process(IMemberPopulation population)
        {
            if (population.TargetMember.IsSimple && population.TargetMember.ExistingValueCanBeChecked)
            {
                var targetMemberAccess = GetTargetMemberAccess(population);
                var targetMemberHasDefaultValue = targetMemberAccess.GetIsDefaultComparison();

                population.WithCondition(targetMemberHasDefaultValue);
            }
        }

        private static Expression GetTargetMemberAccess(IMemberPopulation population)
            => population.TargetMember.GetAccess(population.ObjectMappingContext.InstanceVariable);
    }
}