namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Guids;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<Ef6TestDbContext>,
        IGuidConversionFailureTest
    {
        public WhenConvertingToGuids(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingAParseableStringToAGuid()
            => RunShouldErrorProjectingAParseableStringToAGuid();

        [Fact]
        public void ShouldErrorProjectingANullStringToAGuid()
            => RunShouldErrorProjectingANullStringToAGuid();
    }
}