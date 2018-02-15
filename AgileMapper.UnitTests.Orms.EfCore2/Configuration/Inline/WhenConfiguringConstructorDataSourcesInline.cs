namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration.Inline;
    using Xunit;

    public class WhenConfiguringConstructorDataSourcesInline 
        : WhenConfiguringConstructorDataSourcesInline<EfCore2TestDbContext>
    {
        public WhenConfiguringConstructorDataSourcesInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantByParameterTypeInline()
            => RunShouldApplyAConfiguredConstantByParameterTypeInline();
    }
}