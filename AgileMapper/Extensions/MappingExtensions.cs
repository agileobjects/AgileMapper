namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;

    /// <summary>
    /// Provides extension methods to map object instances using the default or a specified Mapper.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Perform a mapping operation on this <paramref name="source"/> object using the default 
        /// <see cref="IMapper"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <returns>A TargetSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetSelector<TSource> Map<TSource>(this TSource source) => Mapper.Map(source);

        /// <summary>
        /// Perform a mapping operation on this <paramref name="source"/> object using the given 
        /// <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <param name="mapper">
        /// The <see cref="IMapper"/> instance with which to perform the object creation.
        /// </param>
        /// <returns>A TargetSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetSelector<TSource> MapUsing<TSource>(this TSource source, IMapper mapper)
            => mapper.Map(source);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the default <see cref="IMapper"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(this T instance) => Mapper.Default.DeepClone(instance);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the given <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <param name="mapper">The <see cref="IMapper"/> instance with which to perform the deep clone.</param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepCloneUsing<T>(this T instance, IMapper mapper)
            => mapper.DeepClone(instance);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the default <see cref="IMapper"/> and
        /// the given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(
            this T instance,
            params Expression<Action<IFullMappingInlineConfigurator<T, T>>>[] configurations)
        {
            return Mapper.Default.DeepClone(instance, configurations);
        }

        /// <summary>
        /// Flatten the given <paramref name="source"/> object so it has only value-type or string members,
        /// using the default Mapper.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <returns>A FlatteningTypeSelector with which to select the type of flattening to perform.</returns>
        public static IFlatteningSelector<TSource> Flatten<TSource>(this TSource source)
            => Mapper.Default.Flatten(source);

        /// <summary>
        /// Flatten the given <paramref name="source"/> object so it has only value-type or string members, 
        /// using the given <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <param name="mapper">
        /// The <see cref="IMapper"/> instance with which to perform the flattening.
        /// </param>
        /// <returns>A FlatteningTypeSelector with which to select the type of flattening to perform.</returns>
        public static IFlatteningSelector<TSource> FlattenUsing<TSource>(this TSource source, IMapper mapper)
            => mapper.Flatten(source);
    }
}
