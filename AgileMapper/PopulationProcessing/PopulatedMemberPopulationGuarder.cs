namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class PopulatedMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new PopulatedMemberPopulationGuarder();

        public IEnumerable<MemberPopulation> Process(IEnumerable<MemberPopulation> populations)
        {
            foreach (var population in populations)
            {
                if (population.TargetMember.ExistingValueCanBeChecked &&
                    population.TargetMember.IsSimple)
                {
                    var targetMemberAccess = GetTargetMemberAccess(population);
                    var targetMemberHasDefaultValue = targetMemberAccess.GetIsDefaultComparison();
                    var ifDefaultValueThenPopulate = Expression.IfThen(targetMemberHasDefaultValue, population.Population);

                    yield return population.WithPopulation(ifDefaultValueThenPopulate);
                    continue;
                }

                yield return population;
            }
        }

        private static Expression GetTargetMemberAccess(MemberPopulation population)
            => population.TargetMember.GetAccess(population.ObjectMappingContext.TargetVariable);
    }
}