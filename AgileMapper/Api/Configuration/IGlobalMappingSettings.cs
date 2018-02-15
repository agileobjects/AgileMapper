namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    /// <summary>
    /// Provides options for globally configuring how all mappers will perform mappings.
    /// </summary>
    public interface IGlobalMappingSettings
    {
        #region Exception Handling

        /// <summary>
        /// Swallow exceptions thrown during a mapping, for all source and target types. Object mappings which 
        /// encounter an Exception will return null.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings SwallowAllExceptions();

        /// <summary>
        /// Pass Exceptions thrown during a mapping to the given <paramref name="callback"/> instead of throwing 
        /// them, for all source and target types.
        /// </summary>
        /// <param name="callback">
        /// The callback to which to pass thrown Exception information. If the thrown exception should not be 
        /// swallowed, it should be rethrown inside the callback.
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings PassExceptionsTo(Action<IMappingExceptionData> callback);

        #endregion

        #region Naming

        /// <summary>
        /// Expect members of all source and target types to potentially have the given name <paramref name="prefix"/>.
        /// Source and target members will be matched as if the prefix is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNamePrefix(string prefix);

        /// <summary>
        /// Expect members of all source and target types to potentially have any of the given name <paramref name="prefixes"/>.
        /// Source and target members will be matched as if the prefixes are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNamePrefixes(params string[] prefixes);

        /// <summary>
        /// Expect members of all source and target types to potentially have the given name <paramref name="suffix"/>.
        /// Source and target members will be matched as if the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNameSuffix(string suffix);

        /// <summary>
        /// Expect members of all source and target types to potentially have any of the given name <paramref name="suffixes"/>.
        /// Source and target members will be matched as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNameSuffixes(params string[] suffixes);

        /// <summary>
        /// Expect members of all source and target types to potentially match the given name <paramref name="pattern"/>.
        /// The pattern will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and target member names. The pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNamePattern(string pattern);

        /// <summary>
        /// Expect members of all source and target types to potentially match the given name <paramref name="patterns"/>.
        /// The patterns will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and target member names. Each pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings UseNamePatterns(params string[] patterns);

        #endregion

        /// <summary>
        /// Ensure 1-to-1 relationships between source and mapped objects by tracking and reusing mapped objects if 
        /// they appear more than once in a source object tree. Mapped objects are automatically tracked in object 
        /// trees with circular relationships - unless <see cref="DisableObjectTracking"/> is called - so configuring 
        /// this option is not necessary just to map circular relationships.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings MaintainIdentityIntegrity();

        /// <summary>
        /// Disable tracking of objects during circular relationship mapping between all source and target types. 
        /// Mapped objects are tracked by default when mapping circular relationships to prevent stack overflows 
        /// if two objects in a source object tree hold references to each other, and to ensure 1-to-1 relationships 
        /// between source and mapped objects. If you are confident that each object in a source object tree appears 
        /// only once, disabling object tracking will increase mapping performance.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings DisableObjectTracking();

        /// <summary>
        /// Map null source collections to null instead of an empty collection, for all source and target types.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalMappingSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        IGlobalMappingSettings MapNullCollectionsToNull();

        /// <summary>
        /// Gets a link back to the full <see cref="MappingConfigStartingPoint"/>, for api fluency.
        /// </summary>
        MappingConfigStartingPoint AndWhenMapping { get; }
    }
}