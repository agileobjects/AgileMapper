namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDoubles :
        WhenConvertingToDoubles<EfCore1TestDbContext>,
        IStringConverterTest<int>,
        IStringConversionValidationFailureTest<int>
    {
        public WhenConvertingToDoubles(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToADouble();

        [Fact]
        public void ShouldProjectANullString()
            => RunShouldProjectANullStringToADouble();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADouble();
    }
}