namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains details of the mapping plan for a mapping between a particular source and target type,
    /// for all mapping types (create new, merge, overwrite).
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object from which the mapping described by the
    /// <see cref="MappingPlanSet{TSource,TTarget}"/> is performed.
    /// </typeparam>
    /// <typeparam name="TTarget">
    /// The type of target object to which the mapping described by the
    /// <see cref="MappingPlanSet{TSource,TTarget}"/> is performed.
    /// </typeparam>
    public class MappingPlanSet<TSource, TTarget>
    {
        private readonly IEnumerable<MappingPlan<TSource, TTarget>> _mappingPlans;

        internal MappingPlanSet(IEnumerable<MappingPlan<TSource, TTarget>> mappingPlans)
        {
            _mappingPlans = mappingPlans;
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlans">MappingPlanSet</paramref> to its string 
        /// representation.
        /// </summary>
        /// <param name="mappingPlans">The <see cref="MappingPlan{TSource,TTarget}"/> to convert.</param>
        /// <returns>
        /// The string representation of the <paramref name="mappingPlans">MappingPlanSet</paramref>.
        /// </returns>
        public static implicit operator string(MappingPlanSet<TSource, TTarget> mappingPlans)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlans._mappingPlans.Select(plan => (string)plan));
        }

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlanSet{TSource,TTarget}"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlanSet{TSource,TTarget}"/>.</returns>
        public override string ToString() => this;
    }
}