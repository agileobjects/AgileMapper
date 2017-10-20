namespace AgileObjects.AgileMapper.Api
{
    using Plans;

    /// <summary>
    /// Provides options to create and compile mapping functions for a particular type of mapping from the 
    /// source type being configured to a specified target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    public class PlanTargetTypeSelector<TSource>
    {
        private readonly MapperContext _mapperContext;

        internal PlanTargetTypeSelector(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        /// <summary>
        /// Create and compile mapping functions for a create new mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        public string ToANew<TResult>()
            => GetMappingPlan<TResult>(_mapperContext.RuleSets.CreateNew);

        /// <summary>
        /// Create and compile mapping functions for an OnTo (merge) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        public string OnTo<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Merge);

        /// <summary>
        /// Create and compile mapping functions for an Over (overwrite) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        public string Over<TTarget>()
            => GetMappingPlan<TTarget>(_mapperContext.RuleSets.Overwrite);

        private string GetMappingPlan<TTarget>(MappingRuleSet ruleSet)
        {
            var planContext = new MappingExecutor<TSource>(ruleSet, _mapperContext);

            return new MappingPlan<TSource, TTarget>(planContext);
        }
    }
}