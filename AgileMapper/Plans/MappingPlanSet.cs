namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class MappingPlanSet<TSource, TTarget>
    {
        private readonly IEnumerable<MappingPlan<TSource, TTarget>> _mappingPlans;

        public MappingPlanSet(IEnumerable<MappingPlan<TSource, TTarget>> mappingPlans)
        {
            _mappingPlans = mappingPlans;
        }

        public static implicit operator string(MappingPlanSet<TSource, TTarget> mappingPlans)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlans._mappingPlans.Select(plan => (string)plan));
        }
    }
}