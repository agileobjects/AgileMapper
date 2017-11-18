namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDoubles :
        WhenConvertingToDoubles<EfCore2TestDbContext>,
        IStringConverterTest<int>,
        IStringConversionValidationFailureTest<int>
    {
        public WhenConvertingToDoubles(InMemoryEfCore2TestContext context)
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