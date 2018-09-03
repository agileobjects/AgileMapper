namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringStringFormattingInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringStringFormattingInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDecimalsInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var source = new PublicDecimal { Value = 123.00m };

                await context.DecimalItems.AddAsync(source);
                await context.SaveChangesAsync();

                var stringDto = await context
                    .DecimalItems
                    .ProjectUsing(mapper)
                    .To<PublicStringDto>(cfg => cfg
                        .WhenMapping
                        .StringsFrom<decimal>(c => c.FormatUsing("C"))
                        .AndWhenMapping
                        .From<PublicDecimal>()
                        .ProjectedTo<PublicStringDto>()
                        .Map(d => d.Value * 2)
                        .To(dto => dto.Value))
                    .SingleAsync();

                stringDto.Value.ShouldBe(246.00m.ToString("C"));
            });
        }
    }
}
