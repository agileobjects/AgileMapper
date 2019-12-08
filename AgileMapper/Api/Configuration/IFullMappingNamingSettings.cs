namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for configuring member name matching settings.
    /// </summary>
    /// <typeparam name="TNamingSettings">
    /// The type of naming settings object to return in the fluent API.
    /// </typeparam>
    public interface IFullMappingNamingSettings<out TNamingSettings>
    {
        /// <summary>
        /// Expect members of the source and target types being configured to potentially have the
        /// given name <paramref name="prefix"/>. Source and target members will be matched as if
        /// the prefix is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and target members.</param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNamePrefix(string prefix);

        /// <summary>
        /// Expect members of the source and target types being configured to potentially have any
        /// of the given name <paramref name="prefixes"/>. Source and target members will be matched
        /// as if the prefixes are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and target members.</param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNamePrefixes(params string[] prefixes);

        /// <summary>
        /// Expect members of the source and target types being configured to potentially have the
        /// given name <paramref name="suffix"/>. Source and target members will be matched as if
        /// the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and target members.</param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNameSuffix(string suffix);

        /// <summary>
        /// Expect members of the source and target types being configured to potentially have any
        /// of the given name <paramref name="suffixes"/>. Source and target members will be matched
        /// as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and target members.</param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNameSuffixes(params string[] suffixes);

        /// <summary>
        /// Expect members of the source and target types being configured to potentially match the
        /// given name <paramref name="pattern"/>. The pattern will be used to find the part of a
        /// name which should be used to match a source and target member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and target member names. The pattern is expected
        /// to start with the ^ character, end with the $ character and contain a single capturing
        /// group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNamePattern(string pattern);

        /// <summary>
        /// Expect members of the source and target types being configured to potentially match the
        /// given name <paramref name="patterns"/>. The patterns will be used to find the part of a
        /// name which should be used to match a source and target member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and target member names. Each pattern is expected
        /// to start with the ^ character, end with the $ character and contain a single capturing group
        /// wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// This IFullMappingNamingSettings with which to configure further settings.
        /// </returns>
        TNamingSettings UseNamePatterns(params string[] patterns);
    }
} 