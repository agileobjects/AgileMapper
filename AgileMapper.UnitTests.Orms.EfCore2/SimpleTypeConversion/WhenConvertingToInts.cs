namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<EfCore2TestDbContext>,
        IStringConverterTest,
        IStringConversionValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToAnInt();

        [Fact]
        public void ShouldProjectANullString()
            => RunShouldProjectANullStringToAnInt();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToAnInt();
    }
}