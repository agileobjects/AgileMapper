namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringConstructorDataSources : WhenConfiguringConstructorDataSources<Ef6TestDbContext>
    {
        public WhenConfiguringConstructorDataSources(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorApplyingAConfiguredConstantByParameterType()
            => RunShouldErrorApplyingAConfiguredConstantByParameterType();

        [Fact]
        public Task ShouldErrorApplyingAConfiguredExpressionByParameterName()
            => RunShouldErrorApplyingAConfiguredExpressionByParameterName();
    }
}