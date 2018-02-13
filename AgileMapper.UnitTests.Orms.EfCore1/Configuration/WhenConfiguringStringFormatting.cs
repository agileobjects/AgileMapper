namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringStringFormatting : WhenConfiguringStringFormatting<EfCore1TestDbContext>
    {
        public WhenConfiguringStringFormatting(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDateTimes() => DoShouldFormatDateTimes(d => d.ToString("o"));

        [Fact]
        public Task ShouldFormatDecimals() => DoShouldFormatDecimals(d => d.ToString("C"));
    }
}
