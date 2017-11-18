namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<EfCore1TestDbContext>,
        IStringConverterTest,
        IStringConversionValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEfCore1TestContext context)
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