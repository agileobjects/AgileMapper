namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.LocalDb.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Orms.Infrastructure;
    using Xunit;

    public class WhenConfiguringStringFormatting : WhenConfiguringStringFormatting<Ef5TestLocalDbContext>
    {
        public WhenConfiguringStringFormatting(LocalDbTestContext<Ef5TestLocalDbContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldFormatDateTimes()
            => DoShouldFormatDateTimes(d => d.ToString("yyyy-%M-%d %H:%m:%s"));

        [Fact]
        public Task ShouldFormatDecimals() => DoShouldFormatDecimals(d => d.ToString(".000000"));
    }
}
