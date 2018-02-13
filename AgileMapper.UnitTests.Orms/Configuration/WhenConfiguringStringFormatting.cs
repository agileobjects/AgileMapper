namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConfiguringStringFormatting<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringStringFormatting(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        protected Task DoShouldFormatDateTimes(Func<DateTime, string> expectedDateStringFactory)
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

                stringDto.Value.ShouldBe(expectedDateStringFactory.Invoke(source.Value));
            });
        }

        protected Task DoShouldFormatDecimals(Func<decimal, string> expectedDecimalStringFactory)
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .StringsFrom<decimal>(c => c.FormatUsing("C"));

                var source = new PublicDecimal { Value = 674378.52m };
                var result = mapper.Map(source).ToANew<PublicStringDto>();

                result.Value.ShouldBe(source.Value.ToString("C"));

                await context.DecimalItems.Add(source);
                await context.SaveChanges();

                var stringDto = context
                    .DecimalItems
                    .ProjectUsing(mapper)
                    .To<PublicStringDto>()
                    .ShouldHaveSingleItem();

                stringDto.Value.ShouldBe(expectedDecimalStringFactory.Invoke(source.Value));
            });
        }
    }
}
