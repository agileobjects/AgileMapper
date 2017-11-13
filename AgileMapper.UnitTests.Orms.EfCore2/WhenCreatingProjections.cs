namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Linq;
    using Infrastructure;
    using MoreTestClasses;
    using Orms.Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCreatingProjections : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenCreatingProjections(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldReuseACachedProjectionMapper()
        {
            RunTest((context, mapper) =>
            {
                var stringDtos = context
                    .StringItems
                    .ProjectTo<PublicStringDto>(c => c.Using(mapper))
                    .ToArray();

                stringDtos.ShouldBeEmpty();

                context.StringItems.Add(new PublicString { Id = 1, Value = "New!" });
                context.SaveChanges();

                var moreStringDtos = context
                    .StringItems
                    .ProjectTo<PublicStringDto>(c => c.Using(mapper))
                    .ToArray();

                moreStringDtos.ShouldHaveSingleItem();

                mapper.RootMapperCountShouldBeOne();
            });
        }
    }
}
