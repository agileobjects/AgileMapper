namespace AgileObjects.AgileMapper.UnitTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class MappingExtensions
    {
        public static object RootMapperCountShouldBeOne(this IMapper mapper)
            => RootMapperCountShouldBe(mapper, 1).First();

        public static ICollection<object> RootMapperCountShouldBe(this IMapper mapper, int expected)
        {
            var rootMappers = ((Mapper)mapper).Context.ObjectMapperFactory.RootMappers.ToArray();

            if (rootMappers.Length == expected)
            {
                return rootMappers;
            }

            throw new Exception($"Expected {expected} mappers, got {rootMappers.Length}");
        }
    }
}
