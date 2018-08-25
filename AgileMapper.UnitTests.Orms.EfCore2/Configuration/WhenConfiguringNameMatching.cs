namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringNameMatching : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringNameMatching(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseACustomPrefix()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping.UseNamePrefix("_");

                var names = new PublicStringNames
                {
                    _Value = "123",
                    _Value_ = "456",
                    Value_ = "789"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>()
                    .FirstAsync();

                stringDto.Value.ShouldBe("123");
            });
        }

        [Fact]
        public Task ShouldUseACustomSuffix()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping.UseNameSuffix("_");

                var names = new PublicStringNames
                {
                    _Value = "123",
                    _Value_ = "456",
                    Value_ = "789"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>()
                    .FirstAsync();

                stringDto.Value.ShouldBe("789");
            });
        }

        [Fact]
        public Task ShouldUseACustomNamingPattern()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping.UseNamePattern("^_(.+)_$");

                var names = new PublicStringNames
                {
                    _Value = "123",
                    _Value_ = "456",
                    Value_ = "789"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>()
                    .FirstAsync();

                stringDto.Value.ShouldBe("456");
            });
        }
    }
}
