namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
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
        public Task ShouldReuseACachedProjectionMapper()
        {
            return RunTest(async mapper =>
            {
                using (var context1 = new EfCore2TestDbContext())
                {
                    var stringDtos = await context1
                        .StringItems
                        .Project().To<PublicStringDto>(c => c.Using(mapper))
                        .ToListAsync();

                    stringDtos.ShouldBeEmpty();
                }

                mapper.RootMapperCountShouldBeOne();

                using (var context2 = new EfCore2TestDbContext())
                {
                    context2.StringItems.Add(new PublicString { Id = 1, Value = "New!" });
                    await context2.SaveChangesAsync();

                    var moreStringDtos = await context2
                        .StringItems
                        .Project().To<PublicStringDto>(c => c.Using(mapper))
                        .ToArrayAsync();

                    moreStringDtos.ShouldHaveSingleItem();
                }

                mapper.RootMapperCountShouldBeOne();
            });
        }
    }
}
