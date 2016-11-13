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
        /// <param name="membersSource">
        /// An object providing access to objects describing the element members being mapped
        /// from and to.
        /// </param>
        /// <param name="parent">
        /// An object representing the parent <see cref="IObjectMappingData{TSource, TTarget}"/>.
        /// </param>
        /// <returns>An element <see cref="IObjectMappingData{TSource, TTarget}"/> instance.</returns>
        object ForElement<TSource, TTarget>(object membersSource, object parent);
    }
}