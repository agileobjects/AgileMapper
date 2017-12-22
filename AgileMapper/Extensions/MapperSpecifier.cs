namespace AgileObjects.AgileMapper.Extensions
{
    using System;

    /// <summary>
    /// Enables specification of a particular Mapper with which to perform a mapping
    /// via an extension method.
    /// </summary>
    public class MapperSpecifier
    {
        private static readonly MapperSpecifier _instance = new MapperSpecifier();

        private MapperSpecifier()
        {
        }

        internal static IMapperInternal Get(Func<MapperSpecifier, IMapper> mapperSpecifier)
            => (IMapperInternal)mapperSpecifier.Invoke(_instance);

        /// <summary>
        /// Use the given <paramref name="mapper"/> to perform the mapping action.
        /// </summary>
        /// <param name="mapper">The <see cref="IMapper"/> to use.</param>
        /// <returns>The <see cref="IMapper"/> to use.</returns>
        public IMapper Using(IMapper mapper) => mapper;
    }
}