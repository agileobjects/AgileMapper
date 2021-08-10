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
        /// Gets the name of the rule set (CreateNew, Overwrite, etc) used to create the mapping
        /// described by this <see cref="IMappingPlan"/>.
        /// </summary>
        string RuleSetName { get; }

        /// <summary>
        /// Gets the root <see cref="IMappingPlan"/> describing the plan to map the root source
        /// and target objects.
        /// </summary>
        IMappingPlanFunction Root { get; }

        /// <summary>
        /// Gets a source-code string translation of this <see cref="IMappingPlan"/>.
        /// </summary>
        /// <returns>A source-code string translation of this <see cref="IMappingPlan"/>.</returns>
        string ToSourceCode();
    }
}