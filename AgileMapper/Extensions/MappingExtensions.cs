namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;

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

        /// <summary>
        /// Perform a deep clone of this <paramref name="instance"/> using the given 
        /// <paramref name="configurations"/>.
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

        private static T DeepClone<T>(T instance, IMapper mapper) => mapper.DeepClone(instance);
    }
}
