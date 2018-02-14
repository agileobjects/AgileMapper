namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringStringFormatting : WhenConfiguringStringFormatting<Ef6TestDbContext>
    {
        public WhenConfiguringStringFormatting(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDateTimes()
            => DoShouldFormatDateTimes(d => d.ToString("MM/dd/yyyy HH:mm:ss"));

        [Fact]
        public Task ShouldFormatDecimals() => DoShouldFormatDecimals(d => d + "");

        [Fact]
        public Task ShouldFormatDoubles() => DoShouldFormatDoubles(d => d + "");
    }
}
