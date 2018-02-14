namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringStringFormatting : WhenConfiguringStringFormatting<EfCore2TestDbContext>
    {
        public WhenConfiguringStringFormatting(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDateTimes() => DoShouldFormatDateTimes();

        [Fact]
        public Task ShouldFormatDecimals() => DoShouldFormatDecimals();

        [Fact]
        public Task ShouldFormatDoubles() => DoShouldFormatDoubles();
    }
}
