namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<EfCore2TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstant() => DoShouldApplyAConfiguredConstant();

        [Fact]
        public Task ShouldConditionallyApplyAConfiguredConstant()
            => DoShouldConditionallyApplyAConfiguredConstant();

        [Fact]
        public Task ShouldApplyAConfiguredConstantToANestedMember()
            => DoShouldApplyAConfiguredConstantToANestedMember();

        [Fact]
        public Task ShouldApplyAConfiguredMember() => DoShouldApplyAConfiguredMember();
    }
}