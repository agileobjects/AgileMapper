namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToInts :
        WhenConvertingToInts<Ef6TestDbContext>,
        IStringConversionFailureTest,
        IStringConversionValidationFailureTest
    {
        public WhenConvertingToInts(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToAnInt();

        [Fact]
        public Task ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToAnInt();

        [Fact]
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToAnInt();
    }
}