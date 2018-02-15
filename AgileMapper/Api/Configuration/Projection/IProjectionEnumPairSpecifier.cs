namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for specifying the enum member(s) with which the previously-specified enum member(s) 
    /// should be paired.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    /// <typeparam name="TTarget">The target type being configured.</typeparam>
    public interface IProjectionEnumPairSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Configure this mapper to project the previously-specified enum member(s) to the given 
        /// <paramref name="pairedEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TPairedEnum">The type of enum the members of which are being paired.</typeparam>
        /// <param name="pairedEnumMembers">
        /// One or more enum members to pair to the previously-specified enum members.
        /// </param>
        /// <returns>An IProjectionConfigContinuation with which to configure other aspects of mapping.</returns>
        IProjectionConfigContinuation<TSource, TTarget> With<TPairedEnum>(params TPairedEnum[] pairedEnumMembers)
            where TPairedEnum : struct;
    }
}