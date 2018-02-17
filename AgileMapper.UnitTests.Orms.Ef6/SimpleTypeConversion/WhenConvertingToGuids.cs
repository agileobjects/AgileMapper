namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.SimpleTypeConversion;
    using Xunit;

    public class WhenConvertingToGuids : WhenConvertingToGuids<Ef6TestDbContext>
    {
        public WhenConvertingToGuids(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorProjectingAParseableString()
            => RunShouldErrorProjectingAParseableStringToAGuid();

        [Fact]
        public Task ShouldErrorProjectingANullString()
            => RunShouldErrorProjectingANullStringToAGuid();
    }
}