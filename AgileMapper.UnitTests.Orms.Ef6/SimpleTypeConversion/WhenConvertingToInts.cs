namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Integers;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<Ef6TestDbContext>,
        IStringToIntegerConversionFailureTest,
        IStringToIntegerValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEf6TestContext context)
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