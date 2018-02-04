namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDoubles :
        WhenConvertingToDoubles<Ef6TestDbContext>,
        IStringConversionFailureTest,
        IStringConversionValidationFailureTest

    {
        public WhenConvertingToDoubles(InMemoryEf6TestContext context)
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