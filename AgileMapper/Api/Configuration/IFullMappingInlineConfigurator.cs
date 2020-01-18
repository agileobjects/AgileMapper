namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Reflection;
    using Dictionaries;
#if FEATURE_DYNAMIC
    using Dynamics;
#endif

    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingInlineConfigurator<TSource, TTarget> :
        IFullMappingSettings<TSource, TTarget>,
        IFullMappingConfigStartingPoint<TSource, TTarget>,
        IFullMappingNamingSettings<IFullMappingInlineConfigurator<TSource, TTarget>>
    {
        /// <summary>
        /// Configure how this mapper performs a mapping, inline. Use this property to switch from 
        /// configuration of the root Types on which the mapping is being performed to configuration 
        /// of any other Types.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }

        /// <summary>
        /// Configure how this mapper performs a target Dictionary mapping, inline. Use this property 
        /// to access Dictionary-specific configuration; custom member keys, separators, etc.
        /// </summary>
        ITargetDictionaryMappingInlineConfigurator<TSource, TTarget> ForDictionaries { get; }

#if FEATURE_DYNAMIC
        /// <summary>
        /// Configure how this mapper performs a target ExpandoObject mapping, inline. Use this property 
        /// to access ExpandoObject-specific configuration; separators, etc.
        /// </summary>
        ITargetDynamicMappingInlineConfigurator<TSource> ForDynamics { get; }
#endif
        /// <summary>
        /// Throw an exception upon execution of this statement if the mapping being configured has any target members 
        /// which will not be mapped, maps from a source enum to a target enum which does not support all of its values,
        /// or includes complex types which cannot be constructed. Use calls to this method to validate a mapping plan; 
        /// remove them in production code.
        /// </summary>
        void ThrowNowIfMappingPlanIsIncomplete();

        /// <summary>
        /// Scan the given <paramref name="assemblies"/> when looking for types derived from any source or 
        /// target type being mapped.
        /// </summary>
        /// <param name="assemblies">The assemblies in which to look for derived types.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> LookForDerivedTypesIn(params Assembly[] assemblies);
    }
}