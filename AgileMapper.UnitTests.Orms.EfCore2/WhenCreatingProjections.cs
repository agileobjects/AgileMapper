namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
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
                        .ProjectUsing(mapper).To<PublicStringDto>()
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
                        .ProjectUsing(mapper).To<PublicStringDto>()
                        .ToArrayAsync();

                    moreStringDtos.ShouldHaveSingleItem();
                }

                mapper.RootMapperCountShouldBeOne();
            });
        }

        [Fact]
        public Task ShouldMapAQueryableAsAnEnumerable()
        {
            return RunTest(async (context, mapper) =>
            {
                await context.BoolItems.AddRangeAsync(new PublicBool { Value = true }, new PublicBool { Value = false });
                await context.SaveChangesAsync();

                var result = mapper
                    .Map(context.BoolItems.Where(bi => bi.Value))
                    .ToANew<List<PublicBoolDto>>();

                result.ShouldNotBeNull();
                result.ShouldHaveSingleItem().Value.ShouldBeTrue();
            });
        }
    }
}
