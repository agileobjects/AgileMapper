namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration.Inline;
    using Xunit;

    public class WhenConfiguringConstructorDataSourcesInline
        : WhenConfiguringConstructorDataSourcesInline<Ef5TestDbContext>
    {
        public WhenConfiguringConstructorDataSourcesInline(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorApplyingAConfiguredConstantByParameterTypeInline()
            => RunShouldErrorApplyingAConfiguredConstantByParameterTypeInline();
    }
}