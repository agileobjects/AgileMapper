namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains sets of details of mapping plans for mappings between a particular source and target types,
    /// for particular mapping types (create new, merge, overwrite).
    /// </summary>
    public class MappingPlanSet
    {
        private readonly IEnumerable<MappingPlan> _mappingPlans;

        internal MappingPlanSet(IEnumerable<MappingPlan> mappingPlans)
        {
            _mappingPlans = mappingPlans;
        }

        internal static MappingPlanSet For(MapperContext mapperContext)
        {
            return new MappingPlanSet(mapperContext
                .ObjectMapperFactory
                .RootMappers
                .Select(mapper => new MappingPlan(mapper))
                .ToArray());
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlans">MappingPlanSet</paramref> to its string 
        /// representation.
        /// </summary>
        /// <param name="mappingPlans">The <see cref="MappingPlanSet"/> to convert.</param>
        /// <returns>
        /// The string representation of the <paramref name="mappingPlans">MappingPlanSet</paramref>.
        /// </returns>
        public static implicit operator string(MappingPlanSet mappingPlans)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlans._mappingPlans.Select(plan => plan.ToString()));
        }

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlanSet"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlanSet"/>.</returns>
        public override string ToString() => this;
    }
}