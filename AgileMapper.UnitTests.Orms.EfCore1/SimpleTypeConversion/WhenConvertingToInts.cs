namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Integers;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<EfCore1TestDbContext>,
        IStringToIntegerConverterTest,
        IStringToIntegerValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEfCore1TestContext context)
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