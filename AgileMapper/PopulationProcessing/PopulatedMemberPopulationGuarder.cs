namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class PopulatedMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new PopulatedMemberPopulationGuarder();

        public IEnumerable<IMemberPopulation> Process(IEnumerable<IMemberPopulation> populations)
        {
            foreach (var population in populations)
            {
                if (population.TargetMember.ExistingValueCanBeChecked &&
                    population.TargetMember.IsSimple)
                {
                    var targetMemberAccess = GetTargetMemberAccess(population);
                    var targetMemberHasDefaultValue = targetMemberAccess.GetIsDefaultComparison();

                    yield return population.AddCondition(targetMemberHasDefaultValue);
                    continue;
                }

                yield return population;
            }
        }

        private static Expression GetTargetMemberAccess(IMemberPopulation population)
            => population.TargetMember.GetAccess(population.ObjectMappingContext.InstanceVariable);
    }
}