namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring settings for mappings from and to a given source and result type.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configured settings should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configured settings should apply.</typeparam>
    public interface IFullProjectionSettings<TSourceElement, TResultElement>
        : IConditionalProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Project recursive relationships to the specified <paramref name="recursionDepth"/>.
        /// For example, when projecting a Category entity which has a SubCategories property of Type 
        /// IEnumerable{Category}, a recursion depth of 1 will populate the sub-categories of the sub-categories
        /// of the top-level Category selected; a recursion depth of 2 will populate the sub-categories of the 
        /// sub-categories of the sub-categories of the top-level Category selected, etc. The default is zero,
        /// which only populates the first level of sub-categories.
        /// </summary>
        /// <param name="recursionDepth">The depth to which to populate projected recursive relationships.</param>
        IFullProjectionInlineConfigurator<TSourceElement, TResultElement> RecurseToDepth(int recursionDepth);

        /// <summary>
        /// Configure this mapper to pair the given <paramref name="enumMember"/> with a member of another 
        /// enum Type.
        /// </summary>
        /// <typeparam name="TPairingEnum">The type of the enum member to pair.</typeparam>
        /// <param name="enumMember">The first enum member in the pair.</param>
        /// <returns>
        /// An IProjectionEnumPairSpecifier with which to specify the enum member to which the given 
        /// <paramref name="enumMember"/> should be paired.
        /// </returns>
        IProjectionEnumPairSpecifier<TSourceElement, TResultElement> PairEnum<TPairingEnum>(TPairingEnum enumMember)
            where TPairingEnum : struct;

        /// <summary>
        /// Gets a link back to the full <see cref="IFullProjectionConfigurator{TSource, TTarget}"/>, for 
        /// api fluency.
        /// </summary>
        IFullProjectionConfigurator<TSourceElement, TResultElement> And { get; }
    }
}