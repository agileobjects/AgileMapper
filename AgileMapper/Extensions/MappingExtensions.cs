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
        /// <returns>A TargetTypeSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetTypeSelector<TSource> Map<TSource>(this TSource source) => Mapper.Map(source);

        /// <summary>
        /// Perform a mapping operation on this <paramref name="source"/> object using the <see cref="IMapper"/> 
        /// specified by the <paramref name="mapperSpecifier"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <param name="mapperSpecifier">
        /// A func supplying the <see cref="IMapper"/> instance with which to perform the deep clone.
        /// </param>
        /// <returns>A TargetTypeSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetTypeSelector<TSource> Map<TSource>(
            this TSource source,
            Func<MapperSpecifier, IMapper> mapperSpecifier)
            => mapperSpecifier.Invoke(MapperSpecifier.Instance).Map(source);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the default <see cref="IMapper"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(this T instance) => DeepClone(instance, Mapper.Default);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the <see cref="IMapper"/> 
        /// specified by the <paramref name="mapperSpecifier"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <param name="mapperSpecifier">
        /// A func supplying the <see cref="IMapper"/> instance with which to perform the deep clone.
        /// </param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(this T instance, Func<MapperSpecifier, IMapper> mapperSpecifier)
            => DeepClone(instance, mapperSpecifier.Invoke(MapperSpecifier.Instance));

        private static T DeepClone<T>(T instance, IMapper mapper) => mapper.DeepClone(instance);

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
    }
}
