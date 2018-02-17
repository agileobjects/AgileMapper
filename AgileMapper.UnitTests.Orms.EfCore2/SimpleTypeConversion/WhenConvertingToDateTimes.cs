namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<EfCore2TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectANullableDateTimeToADateTime()
            => DoShouldProjectANullableDateTimeToADateTime();

        [Fact]
        public Task ShouldProjectANullNullableDateTimeToADateTime()
            => DoShouldProjectANullNullableDateTimeToADateTime();

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