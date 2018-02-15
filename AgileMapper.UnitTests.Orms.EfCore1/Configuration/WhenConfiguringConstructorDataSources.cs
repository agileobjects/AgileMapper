namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringConstructorDataSources : WhenConfiguringConstructorDataSources<EfCore1TestDbContext>
    {
        public WhenConfiguringConstructorDataSources(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantByParameterType()
            => RunShouldApplyAConfiguredConstantByParameterType();

        [Fact]
        public Task ShouldApplyAConfiguredExpressionByParameterName()
            => RunShouldApplyAConfiguredExpressionByParameterName();
    }
}