namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDoubles :
        WhenConvertingToDoubles<Ef5TestDbContext>,
        IStringConversionFailureTest,
        IStringConversionValidationFailureTest

    {
        public WhenConvertingToDoubles(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToADouble();

        [Fact]
        public Task ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToADouble();

        [Fact]
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADouble();
    }
}