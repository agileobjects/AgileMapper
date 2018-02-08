namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    /// <summary>
    /// Provides options for configuring settings for mappings from and to a given source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configured settings should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configured settings should apply.</typeparam>
    public interface IFullMappingSettings<TSource, TTarget> : IConditionalMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Swallow exceptions thrown during a mapping from and to the source and target types being configured. 
        /// Object mappings which encounter an Exception will return null.
        /// </summary>
        /// <returns>
        /// An IFullMappingSettings{TSource, TTarget} with which to configure further settings for the source and
        /// target types being configured.
        /// </returns>
        IFullMappingSettings<TSource, TTarget> SwallowAllExceptions();

        /// <summary>
        /// Pass Exceptions thrown during a mapping from and to the source and target types being configured to 
        /// the given <paramref name="callback"/> instead of throwing them.
        /// </summary>
        /// <param name="callback">
        /// The callback to which to pass thrown Exception information. If the thrown exception should not be 
        /// swallowed, it should be rethrown inside the callback.
        /// </param>
        /// <returns>
        /// An IFullMappingSettings{TSource, TTarget} with which to configure further settings for the source and
        /// target types being configured.
        /// </returns>
        IFullMappingSettings<TSource, TTarget> PassExceptionsTo(Action<IMappingExceptionData<TSource, TTarget>> callback);

        /// <summary>
        /// Ensure 1-to-1 relationships between source and mapped objects during a mapping from and to the source and 
        /// target types being configured, by tracking and reusing mapped objects if they appear more than once in a 
        /// source object tree. Mapped objects are automatically tracked in object trees with circular relationships - 
        /// unless <see cref="DisableObjectTracking"/> is called - so configuring this option is not necessary just to 
        /// map circular relationships.
        /// </summary>
        /// <returns>
        /// An <see cref="IFullMappingSettings{TSource, TTarget}"/> with which to configure further settings for the source 
        /// and target types being configured.
        /// </returns>
        IFullMappingSettings<TSource, TTarget> MaintainIdentityIntegrity();

        /// <summary>
        /// Disable tracking of objects during circular relationship mapping from and to the source and target types 
        /// being configured. Mapped objects are tracked by default when mapping circular relationships to prevent stack 
        /// overflows if two objects in a source object tree hold references to each other, and to ensure 1-to-1 relationships 
        /// between source and mapped objects. If you are confident that each object in a source object tree appears 
        /// only once, disabling object tracking will increase mapping performance.
        /// </summary>
        /// <returns>
        /// An <see cref="IFullMappingSettings{TSource, TTarget}"/> with which to configure further settings for the source 
        /// and target types being configured.
        /// </returns>
        IFullMappingSettings<TSource, TTarget> DisableObjectTracking();

        /// <summary>
        /// Map null source collections to null instead of an empty collection, for the source and target types 
        /// being configured.
        /// </summary>
        /// <returns>
        /// An IFullMappingSettings{TSource, TTarget} with which to configure further settings for the source and
        /// target types being configured.
        /// </returns>
        IFullMappingSettings<TSource, TTarget> MapNullCollectionsToNull();

        /// <summary>
        /// Configure this mapper to pair the given <paramref name="enumMember"/> with a member of another 
        /// enum Type.
        /// </summary>
        /// <typeparam name="TPairingEnum">The type of the enum member to pair.</typeparam>
        /// <param name="enumMember">The first enum member in the pair.</param>
        /// <returns>
        /// An IMappingEnumPairSpecifier with which to specify the enum member to which the given 
        /// <paramref name="enumMember"/> should be paired.
        /// </returns>
        IMappingEnumPairSpecifier<TSource, TTarget> PairEnum<TPairingEnum>(TPairingEnum enumMember)
            where TPairingEnum : struct;

        /// <summary>
        /// Gets a link back to the full <see cref="IFullMappingConfigurator{TSource, TTarget}"/>, for api fluency.
        /// </summary>
        IFullMappingConfigurator<TSource, TTarget> And { get; }
    }
}