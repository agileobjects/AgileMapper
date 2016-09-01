namespace AgileObjects.AgileMapper.Api
{
    /// <summary>
    /// Provides options to specify the type of mapping to perform.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    public class TargetTypeSelector<TSource>
    {
        private readonly TSource _source;
        private readonly MapperContext _mapperContext;

        internal TargetTypeSelector(TSource source, MapperContext mapperContext)
        {
            _source = source;
            _mapperContext = mapperContext;
        }

        /// <summary>
        /// Perform a new object mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <returns>The result of the new object mapping.</returns>
        public TResult ToANew<TResult>() where TResult : class
            => PerformMapping(_mapperContext.RuleSets.CreateNew, default(TResult));

        /// <summary>
        /// Perform an OnTo (merge) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        public TTarget OnTo<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(_mapperContext.RuleSets.Merge, existing);

        /// <summary>
        /// Perform an Over (overwrite) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        public TTarget Over<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(_mapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget existing)
        {
            using (var mappingContext = new MappingContext(ruleSet, _mapperContext))
            {
                return mappingContext.MapStart(_source, existing);
            }
        }
    }
}