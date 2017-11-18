namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Integers;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<Ef5TestDbContext>,
        IStringToIntegerConversionFailureTest,
        IStringToIntegerValidationFailureTest

    {
        public WhenConvertingToInts(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAParseableStringToAnInt()
            => RunShouldErrorProjectingAParseableStringToAnInt();

        [Fact]
        public void ShouldErrorProjectingANullStringToAnInt()
            => RunShouldErrorProjectingANullStringToAnInt();

        [Fact]
        public void ShouldErrorProjectingAnUnparseableStringToAnInt()
            => RunShouldErrorProjectingAnUnparseableStringToAnInt();
    }
}