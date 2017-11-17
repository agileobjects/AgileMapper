namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.SimpleTypeConversion.Guids;
    using Xunit;

    public class WhenConvertingToGuids :
        WhenConvertingToGuids<EfCore1TestDbContext>,
        IGuidConverterTest
    {
        public WhenConvertingToGuids(InMemoryEfCore1TestContext context)
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