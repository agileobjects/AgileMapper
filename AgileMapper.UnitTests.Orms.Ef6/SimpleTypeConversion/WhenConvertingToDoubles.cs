namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
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
        public void ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToADouble();

        [Fact]
        public void ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToADouble();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADouble();
    }
}