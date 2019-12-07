namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Members;

    /// <summary>
    /// Provides options for globally configuring how all mappers will perform mappings.
    /// </summary>
    public interface IGlobalMappingSettings : IFullMappingNamingSettings<IGlobalMappingSettings>
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
        /// Setup Mapper configuration via <see cref="MapperConfiguration"/> instances.
        /// </summary>
        MapperConfigurationSpecifier UseConfigurations { get; }

        /// <summary>
        /// Gets a link back to the full <see cref="MappingConfigStartingPoint"/>, for api fluency.
        /// </summary>
        MappingConfigStartingPoint AndWhenMapping { get; }
    }
}