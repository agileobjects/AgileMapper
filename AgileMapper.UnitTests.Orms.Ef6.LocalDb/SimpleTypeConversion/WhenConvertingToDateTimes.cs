namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.LocalDb.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDateTimes :
        WhenConvertingToDateTimes<Ef6TestLocalDbContext>,
        IStringConverterTest,
        IStringConversionValidatorTest
    {
        public WhenConvertingToDateTimes(LocalDbTestContext<Ef6TestLocalDbContext> context)
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
        public Task ShouldProjectAnUnparseableString()
            => RunShouldProjectAnUnparseableStringToADateTime();
    }
}