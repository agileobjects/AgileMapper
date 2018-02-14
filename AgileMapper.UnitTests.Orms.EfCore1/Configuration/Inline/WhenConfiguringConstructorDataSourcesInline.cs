namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration.Inline;
    using Xunit;

    public class WhenConfiguringConstructorDataSourcesInline 
        : WhenConfiguringConstructorDataSourcesInline<EfCore1TestDbContext>
    {
        public WhenConfiguringConstructorDataSourcesInline(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantByParameterTypeInline()
            => RunShouldApplyAConfiguredConstantByParameterTypeInline();
    }
}