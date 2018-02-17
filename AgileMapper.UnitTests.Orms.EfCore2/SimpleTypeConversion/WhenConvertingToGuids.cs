namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids : WhenConvertingToGuids<EfCore2TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString() => RunShouldProjectAParseableStringToAGuid();

        [Fact]
        public Task ShouldProjectANullString() => RunShouldProjectANullStringToAGuid();
    }
}