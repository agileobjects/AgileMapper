namespace AgileObjects.AgileMapper.UnitTests.MoreTestClasses
{
    using System.Linq;
    using Shouldly;

    public static class MappingExtensions
    {
        public static void RootMapperCountShouldBeOne(this IMapper mapper)
        {
            RootMapperCountShouldBe(mapper, 1);
        }

        public static void RootMapperCountShouldBe(this IMapper mapper, int expected)
        {
            ((Mapper)mapper).Context.ObjectMapperFactory.RootMappers.Count().ShouldBe(expected);
        }
    }
}
