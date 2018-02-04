namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring query projections from and to given source and target element Types, inline.
    /// </summary>
    /// <typeparam name="TSourceElement">The source element Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result element Type to which the configuration should apply.</typeparam>
    public interface IFullProjectionInlineConfigurator<TSourceElement, TResultElement>
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
    }
}
