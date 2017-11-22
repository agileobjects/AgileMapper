namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class InlineMappingExtensions
    {
        public static IList<MapperContext> InlineContexts(this IMapper mapper)
        {
            return ((Mapper)mapper).Context.InlineContexts.ToArray();
        }
    }
}
