namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and result element Types.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionConfigurator<TSourceElement, TResultElement>
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
        /// Configure a constant value for a particular target member when projecting from and to the source and 
        /// result types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <param name="value">The constant value to map to the configured result member.</param>
        /// <returns>
        /// A CustomDataSourceTargetMemberSpecifier with which to specify the result member to which the custom 
        /// constant value should be applied.
        /// </returns>
        CustomDataSourceTargetMemberSpecifier<TSourceElement, TResultElement> Map<TSourceValue>(TSourceValue value);
    }
}
