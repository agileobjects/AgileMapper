namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for specifying the enum member with which the configured enum member should be paired.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    /// <typeparam name="TTarget">The target type being configured.</typeparam>
    public interface IMappingEnumPairSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Configure this mapper to map the specified first enum member to the given <paramref name="secondEnumMember"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMember">The second enum member in the pair.</param>
        /// <returns>An IMappingConfigContinuation with which to configure other aspects of mapping.</returns>
        IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(TSecondEnum secondEnumMember)
            where TSecondEnum : struct;

        /// <summary>
        /// Configure this mapper to map the previously-specified set of enum members to the given 
        /// <paramref name="secondEnumMembers"/>.
        /// </summary>
        /// <typeparam name="TSecondEnum">The type of the second enum being paired.</typeparam>
        /// <param name="secondEnumMembers">The second set of enum members in the pairs.</param>
        /// <returns>An IMappingConfigContinuation with which to configure other aspects of mapping.</returns>
        IMappingConfigContinuation<TSource, TTarget> With<TSecondEnum>(params TSecondEnum[] secondEnumMembers)
            where TSecondEnum : struct;
    }
}