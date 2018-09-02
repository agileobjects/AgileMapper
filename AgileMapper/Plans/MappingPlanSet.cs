namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;

    /// <summary>
    /// Contains sets of details of mapping plans for mappings between a particular source and target types,
    /// for particular mapping types (create new, merge, overwrite).
    /// </summary>
    public class MappingPlanSet
    {
        private readonly IEnumerable<IMappingPlan> _mappingPlans;

        internal MappingPlanSet(IEnumerable<IMappingPlan> mappingPlans)
        {
            _mappingPlans = mappingPlans;
        }

        internal static MappingPlanSet For(MapperContext mapperContext)
        {
            return new MappingPlanSet(mapperContext
                .ObjectMapperFactory
                .RootMappers
                .Project(mapper => new MappingPlan(mapper))
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
            return mappingPlans
                ._mappingPlans
                .Project(plan => plan.ToString())
                .Join(Environment.NewLine + Environment.NewLine);
        }

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlanSet"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlanSet"/>.</returns>
        public override string ToString() => this;
    }
}