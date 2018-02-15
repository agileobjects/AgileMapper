namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringNameMatchingInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringNameMatchingInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseACustomPrefixInline()
        {
            return RunTest(async (context, mapper) =>
            {
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
                    .To<PublicStringCtorDto>(cfg => cfg.UseNamePrefix("_"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("123");
            });
        }

        [Fact]
        public Task ShouldUseMultipleCustomPrefixesInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var names = new PublicStringNames
                {
                    _Value = "aaa",
                    _Value_ = "bbb",
                    Value_ = "ccc"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>(cfg => cfg.UseNamePrefixes("str", "_"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("aaa");
            });
        }

        [Fact]
        public Task ShouldUseACustomSuffixInline()
        {
            return RunTest(async (context, mapper) =>
            {
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
                    .To<PublicStringCtorDto>(cfg => cfg.UseNameSuffix("_"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("789");
            });
        }

        [Fact]
        public Task ShouldUseMultipleCustomSuffixesInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var names = new PublicStringNames
                {
                    _Value = "aaa",
                    _Value_ = "bbb",
                    Value_ = "ccc"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>(cfg => cfg.UseNameSuffixes("Val", "_"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("ccc");
            });
        }

        [Fact]
        public Task ShouldUseACustomNamingPatternInline()
        {
            return RunTest(async (context, mapper) =>
            {
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
                    .To<PublicStringCtorDto>(cfg => cfg.UseNamePattern("^_(.+)_$"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("456");
            });
        }

        [Fact]
        public Task ShouldUseMultipleCustomNamingPatternsInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var names = new PublicStringNames
                {
                    _Value = "aaa",
                    _Value_ = "bbb",
                    Value_ = "ccc"
                };

                await context.StringNameItems.AddAsync(names);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .StringNameItems
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>(cfg => cfg.UseNamePatterns("^str(.+)Val$", "^_(.+)_$"))
                    .FirstAsync();

                stringDto.Value.ShouldBe("bbb");
            });
        }
    }
}
