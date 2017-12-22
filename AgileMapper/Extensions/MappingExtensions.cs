namespace AgileObjects.AgileMapper.Extensions
{
    using System;

    /// <summary>
    /// Provides extension methods to map object instances using the default or a specified Mapper.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the default <see cref="IMapper"/> 
        /// and return the results.
        /// </summary>
        /// <typeparam name="T">The Type of object to clone.</typeparam>
        /// <param name="instance">The object to clone.</param>
        /// <returns>A deep clone of this <paramref name="instance"/>.</returns>
        public static T DeepClone<T>(this T instance) => DeepClone(instance, Mapper.Default);

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the <see cref="IMapper"/> 
        /// specified by the <paramref name="mapperSpecifier"/> and return the results.
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
    }
}
