namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Linq;
    using Infrastructure;
    using MoreTestClasses;
    using Orms.Infrastructure;
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
            RunTest(mapper =>
            {
                using (var context1 = new EfCore2TestDbContext())
                {
                    var stringDtos = context1
                        .StringItems
                        .ProjectTo<PublicStringDto>(c => c.Using(mapper))
                        .ToArray();

                    stringDtos.ShouldBeEmpty();
                }

                using (var context2 = new EfCore2TestDbContext())
                {
                    context2.StringItems.Add(new PublicString { Id = 1, Value = "New!" });
                    context2.SaveChanges();

                    var moreStringDtos = context2
                        .StringItems
                        .ProjectTo<PublicStringDto>(c => c.Using(mapper))
                        .ToArray();

                    moreStringDtos.ShouldHaveSingleItem();
                }

                mapper.RootMapperCountShouldBeOne();
            });
        }
    }
}
