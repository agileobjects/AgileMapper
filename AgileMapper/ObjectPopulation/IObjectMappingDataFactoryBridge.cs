namespace AgileObjects.AgileMapper.ObjectPopulation
{
    /// <summary>
    /// Provides bridge methods enabling creation of typed <see cref="IObjectMappingData{TSource, TTarget}"/>
    /// instances in partial trust scenarios. This interface is intended for internal use only.
    /// </summary>
    public interface IObjectMappingDataFactoryBridge
    {
        /// <summary>
        /// Creates a child <see cref="IObjectMappingData{TSource, TTarget}"/> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of child source object being mapped from.</typeparam>
        /// <typeparam name="TTarget">The type of child target object being mapped to.</typeparam>
        /// <param name="childMembersSource">
        /// An object providing access to objects describing the child members being mapped
        /// from and to.
        /// </param>
        /// <param name="parent">
        /// An object representing the parent <see cref="IObjectMappingData{TSource, TTarget}"/>.
        /// </param>
        /// <returns>A child <see cref="IObjectMappingData{TSource, TTarget}"/> instance.</returns>
        object ForChild<TSource, TTarget>(object childMembersSource, object parent);

        /// <summary>
        /// Creates an element <see cref="IObjectMappingData{TSource, TTarget}"/> instance.
        /// </summary>
        /// <typeparam name="TSource">The type of source element object being mapped from.</typeparam>
        /// <typeparam name="TTarget">The type of target element object being mapped to.</typeparam>
        /// <param name="parent">
        /// An object representing the parent <see cref="IObjectMappingData{TSource, TTarget}"/>.
        /// </param>
        /// <returns>An element <see cref="IObjectMappingData{TSource, TTarget}"/> instance.</returns>
        object ForElement<TSource, TTarget>(object parent);

        /// <summary>
        /// Creates an <see cref="IObjectMappingData{TSource, TTarget}"/> instance.
        /// </summary>
        /// <typeparam name="TDeclaredSource">
        /// The declared type of child source object being mapped from.
        /// </typeparam>
        /// <typeparam name="TDeclaredTarget">
        /// The declared type of child target object being mapped to.
        /// </typeparam>
        /// <typeparam name="TSource">The actual type of child source object being mapped from.</typeparam>
        /// <typeparam name="TTarget">The actual type of child target object being mapped to.</typeparam>
        /// <param name="source">The source object being mapped from.</param>
        /// <param name="target">The target object being mapped to.</param>
        /// <param name="enumerableIndex">
        /// The index of the current enumerable being mapped, if applicable.
        /// </param>
        /// <param name="mapperKey">A key object uniquely identifying the context being mapped.</param>
        /// <param name="mappingContext">An object describing the context of the current mapping.</param>
        /// <param name="parent">
        /// An <see cref="IObjectMappingData{TSource, TTarget}"/> describing the parent context.
        /// </param>
        /// <returns></returns>
        object CreateMappingData<TDeclaredSource, TDeclaredTarget, TSource, TTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            object mapperKey,
            object mappingContext,
            object parent)
            where TSource : TDeclaredSource
            where TTarget : TDeclaredTarget;
    }
}