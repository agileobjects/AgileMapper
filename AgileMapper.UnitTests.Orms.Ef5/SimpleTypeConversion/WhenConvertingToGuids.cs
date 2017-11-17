namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Guids;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<Ef5TestDbContext>,
        IGuidConversionFailureTest
    {
        public WhenConvertingToGuids(InMemoryEf5TestContext context)
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