namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Guids;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<EfCore2TestDbContext>,
        IGuidConverterTest
    {
        public WhenConvertingToGuids(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableStringToAGuid()
            => RunShouldProjectAParseableStringToAGuid();

        [Fact]
        public void ShouldProjectANullStringToAGuid()
            => RunShouldProjectANullStringToAGuid();
    }
}