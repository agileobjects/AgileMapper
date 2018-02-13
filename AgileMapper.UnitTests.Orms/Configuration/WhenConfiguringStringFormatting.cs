namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringStringFormatting<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringStringFormatting(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDateTimesMapperWide()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .StringsFrom<DateTime>(c => c.FormatUsing("o"));

                var source = new PublicDateTime { Value = DateTime.Now };
                var result = mapper.Map(source).ToANew<PublicStringDto>();

                result.Value.ShouldBe(source.Value.ToString("o"));

                await context.DateTimeItems.Add(source);
                await context.SaveChanges();

                var stringDto = context
                    .DateTimeItems
                    .ProjectUsing(mapper)
                    .To<PublicStringDto>()
                    .ShouldHaveSingleItem();

                stringDto.Value.ShouldBe(source.Value.ToString("o"));
            });
        }
    }
}
