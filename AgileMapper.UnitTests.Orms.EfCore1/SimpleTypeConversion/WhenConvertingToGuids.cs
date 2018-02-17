namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids : WhenConvertingToGuids<EfCore1TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAParseableString() => RunShouldProjectAParseableStringToAGuid();

        [Fact]
        public Task ShouldProjectANullString() => RunShouldProjectANullStringToAGuid();
    }
}