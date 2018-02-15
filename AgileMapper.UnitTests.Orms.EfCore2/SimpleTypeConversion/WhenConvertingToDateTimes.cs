namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<EfCore2TestDbContext>,
        IStringConverterTest,
        IStringConversionValidationFailureTest
    {
        public WhenConvertingToDateTimes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToADateTime();

        [Fact]
        public Task ShouldProjectANullString()
            => RunShouldProjectANullStringToADateTime();

        [Fact]
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADateTime();
    }
}