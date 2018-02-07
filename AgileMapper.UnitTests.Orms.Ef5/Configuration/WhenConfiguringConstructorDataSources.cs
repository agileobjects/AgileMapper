namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringConstructorDataSources : WhenConfiguringConstructorDataSources<Ef5TestDbContext>
    {
        public WhenConfiguringConstructorDataSources(InMemoryEf5TestContext context)
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