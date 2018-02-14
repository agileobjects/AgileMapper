namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and result element Types, inline.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionInlineConfigurator<TSourceElement, TResultElement> :
        IFullProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Configure how this mapper performs a projection, inline. Use this property to switch from 
        /// configuration of the root Types on which the projection is being performed to configuration 
        /// of any other Types.
        /// </summary>
        IProjectionConfigStartingPoint WhenMapping { get; }

        /// <summary>
        /// Throw an exception upon execution of this statement if the projection being configured has any result 
        /// members which will not be mapped, projects from a source enum to a target enum which does not support 
        /// all of its values, or includes complex Types which cannot be constructed. Use calls to this method to 
        /// validate a mapping plan; remove them in production 
        /// code.
        /// </summary>
        void ThrowNowIfMappingPlanIsIncomplete();

        #region Naming

        /// <summary>
        /// Expect members of the source and result element Types being projected to potentially have the given 
        /// name <paramref name="prefix"/>. Source and result element members will be matched as if the prefix 
        /// is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and result element members.</param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNamePrefix(string prefix);

        /// <summary>
        /// Expect members of the source and result element Types being mapped to potentially have any of the given 
        /// name <paramref name="prefixes"/>. Source and result element members will be matched as if the prefixes 
        /// are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and result element members.</param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNamePrefixes(params string[] prefixes);

        /// <summary>
        /// Expect members of the source and result element Types being mapped to potentially have the given name 
        /// <paramref name="suffix"/>. Source and target members will be matched as if the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and result element members.</param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNameSuffix(string suffix);

        /// <summary>
        /// Expect members of the source and result element Types being mapped to potentially have any of the given name 
        /// <paramref name="suffixes"/>. Source and target members will be matched as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and result element members.</param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNameSuffixes(params string[] suffixes);

        /// <summary>
        /// Expect members of the source and result element Types being mapped to potentially match the given name 
        /// <paramref name="pattern"/>. The pattern will be used to find the part of a name which should be used 
        /// to match a source and result element member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and result element member names. The pattern is expected to 
        /// start with the ^ character, end with the $ character and contain a single capturing group wrapped in 
        /// parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNamePattern(string pattern);

        /// <summary>
        /// Expect members of the source and result element Types being mapped to potentially match the given name 
        /// <paramref name="patterns"/>. The patterns will be used to find the part of a name which should be used 
        /// to match a source and result element member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and result element member names. Each pattern is expected to 
        /// start with the ^ character, end with the $ character and contain a single capturing group wrapped in 
        /// parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This <see cref="IFullProjectionInlineConfigurator{TSourceElement, TResultElement}"/> with which to 
        /// configure further settings for the source and result element Types being mapped.
        /// </returns>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> UseNamePatterns(params string[] patterns);

        #endregion
    }
}
