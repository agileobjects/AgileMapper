namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Integers;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<EfCore2TestDbContext>,
        IStringToIntegerConverterTest,
        IStringToIntegerValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableStringToAnInt()
            => RunShouldProjectAParseableStringToAnInt();

        [Fact]
        public void ShouldProjectANullStringToAnInt()
            => RunShouldProjectANullStringToAnInt();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableStringToAnInt()
            => RunShouldErrorProjectingAnUnparseableStringToAnInt();
    }
}