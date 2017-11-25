namespace AgileObjects.AgileMapper.UnitTests.MoreTestClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;

    public static class MappingExtensions
    {
        public static object RootMapperCountShouldBeOne(this IMapper mapper)
        {
            return RootMapperCountShouldBe(mapper, 1).First();
        }

        public static ICollection<object> RootMapperCountShouldBe(this IMapper mapper, int expected)
        {
            var rootMappers = ((Mapper)mapper).Context.ObjectMapperFactory.RootMappers.ToArray();

            rootMappers.Length.ShouldBe(expected);

            return rootMappers;
        }
    }
}
