namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringConstructorDataSources : WhenConfiguringConstructorDataSources<EfCore2TestDbContext>
    {
        public WhenConfiguringConstructorDataSources(InMemoryEfCore2TestContext context)
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