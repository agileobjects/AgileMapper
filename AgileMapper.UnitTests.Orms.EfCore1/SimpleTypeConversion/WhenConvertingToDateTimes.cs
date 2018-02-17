namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<EfCore1TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString()
            => DoShouldProjectAParseableStringToADateTime();

        [Fact]
        public Task ShouldProjectANullString()
            => DoShouldProjectANullStringToADateTime();

        [Fact]
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}