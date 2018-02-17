namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToDoubles : WhenConvertingToDoubles<EfCore2TestDbContext>
    {
        public WhenConvertingToDoubles(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString()
            => RunShouldProjectAParseableStringToADouble();

        [Fact]
        public Task ShouldProjectANullString()
            => RunShouldProjectANullStringToADouble();

        [Fact]
        public Task ShouldErrorProjectingAnUnparseableString()
            => RunShouldErrorProjectingAnUnparseableStringToADouble();
    }
}