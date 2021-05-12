namespace AgileObjects.AgileMapper.Plans
{
    /// <summary>
    /// Provides options to control how a mapping plan should be created.
    /// </summary>
    public class MappingPlanSettings
    {
        internal static readonly MappingPlanSettings EagerPlanned = new()
        {
            LazyCompile = false,
            CommentUnmappableMembers = true,
            CommentUnmappedMembers = true,
            LazyLoadRepeatMappingFuncs = false
        };

        internal static readonly MappingPlanSettings LazyPlanned = new()
        {
            LazyCompile = false,
            CommentUnmappableMembers = false,
            CommentUnmappedMembers = false,
            LazyLoadRepeatMappingFuncs = true
        };

        /// <summary>
        /// Gets or sets a value indicating whether the mapping plan should lazy-compile its
        /// generated Expression tree into the Func with which mapping is performed. If true, the
        /// mapping Func will be compiled from the mapping Expression tree when it is first needed,
        /// instead of when the plan is created. Defaults to false for an upfront-cached mapping
        /// plan.
        /// </summary>
        public bool LazyCompile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include code comments explaining why any
        /// unmappable members cannot be mapped, <i>e.g</i> if a complex type member has no usable
        /// constructor. Defaults to true for an upfront-cached mapping plan.
        /// </summary>
        public bool CommentUnmappableMembers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include code comments noting why any unmapped
        /// members are not mapped, <i>e.g</i> if they've been explicitly ignored, or have no matching
        /// source values. Defaults to true for an upfront-cached mapping plan.
        /// </summary>
        public bool CommentUnmappedMembers { get; set; }

        internal bool LazyLoadRepeatMappingFuncs { get; set; }
    }
}
