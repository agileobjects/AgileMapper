namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Reflection;
    using Dictionaries;
#if DYNAMIC_SUPPORTED
    using Dynamics;
#endif

    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingInlineConfigurator<TSource, TTarget> : IFullMappingConfigurator<TSource, TTarget>
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

#if DYNAMIC_SUPPORTED
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

        #region Naming

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially have the given name 
        /// <paramref name="prefix"/>. Source and target members will be matched as if the prefix is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and target members.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNamePrefix(string prefix);

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially have any of the given name 
        /// <paramref name="prefixes"/>. Source and target members will be matched as if the prefixes are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and target members.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNamePrefixes(params string[] prefixes);

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially have the given name 
        /// <paramref name="suffix"/>. Source and target members will be matched as if the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and target members.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNameSuffix(string suffix);

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially have any of the given name 
        /// <paramref name="suffixes"/>. Source and target members will be matched as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and target members.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNameSuffixes(params string[] suffixes);

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially match the given name 
        /// <paramref name="pattern"/>. The pattern will be used to find the part of a name which should be used to match a 
        /// source and target member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and target member names. The pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNamePattern(string pattern);

        /// <summary>
        /// Expect members of the source and target types being mapped to potentially match the given name 
        /// <paramref name="patterns"/>. The patterns will be used to find the part of a name which should be used to match a 
        /// source and target member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and target member names. Each pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> UseNamePatterns(params string[] patterns);

        #endregion
    }
}