namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes : WhenConvertingToDateTimes<Ef6TestDbContext>
    {
        public WhenConvertingToDateTimes(InMemoryEf6TestContext context)
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
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}