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
        /// Perform a mapping operation on this <paramref name="source"/> object using the <see cref="IMapper"/> 
        /// specified by the <paramref name="mapperSpecifier"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <param name="mapperSpecifier">
        /// A func supplying the <see cref="IMapper"/> instance with which to perform the object creation.
        /// </param>
        /// <returns>A TargetSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetSelector<TSource> Map<TSource>(
            this TSource source,
            Func<MapperSpecifier, IMapper> mapperSpecifier)
        {
            return MapperSpecifier.Get(mapperSpecifier).Map(source);
        }

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the default <see cref="IMapper"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(this T instance) => Mapper.Default.DeepClone(instance);

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
            => MapperSpecifier.Get(mapperSpecifier).DeepClone(instance);

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
        /// Flatten this source object to a flat result Type.
        /// </summary>
        /// <typeparam name="TSource">The Type of object to flatten.</typeparam>
        /// <param name="source">The object instance to flatten.</param>
        /// <returns>A FlattenTypeSelector with which to select the type of flattening to perform.</returns>
        public static IFlatteningSelector<TSource> Flatten<TSource>(this TSource source)
            => new MappingExecutor<TSource>(source, Mapper.Default.Context);

        /// <summary>
        /// Flatten this source object to a flat result Type.
        /// </summary>
        /// <typeparam name="TSource">The Type of object to flatten.</typeparam>
        /// <param name="source">The object instance to flatten.</param>
        /// <param name="mapperSpecifier">
        /// A func supplying the <see cref="IMapper"/> instance with which to perform the flattening.
        /// </param>
        /// <returns>A FlattenTypeSelector with which to select the type of flattening to perform.</returns>
        public static IFlatteningSelector<TSource> Flatten<TSource>(
            this TSource source,
            Func<MapperSpecifier, IMapper> mapperSpecifier)
        {
            return new MappingExecutor<TSource>(source, MapperSpecifier.Get(mapperSpecifier).Context);
        }
    }
}
