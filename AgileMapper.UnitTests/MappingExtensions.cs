namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using ObjectPopulation;

    internal static class MappingExtensions
    {
        public static ICollection<IObjectMapper> RootMappers(this IMapper mapper)
        {
            return ((Mapper)mapper).Context.ObjectMapperFactory.RootMappers.ToArray();
        }
    }
}
