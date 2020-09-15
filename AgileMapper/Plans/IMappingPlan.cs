namespace AgileObjects.AgileMapper.Plans
{
    using System.Collections.Generic;

    /// <summary>
    /// Implementing classes will describe a plan for mapping from one type to another with a
    /// particular rule set.
    /// </summary>
    public interface IMappingPlan : IEnumerable<IMappingPlanFunction>
    {
        /// <summary>
        /// Gets a source-code string translation of this <see cref="IMappingPlan"/>.
        /// </summary>
        /// <returns>A source-code string translation of this <see cref="IMappingPlan"/>.</returns>
        string ToSourceCode();
    }
}