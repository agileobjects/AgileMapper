namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration.Inline;
    using Xunit;

    public class WhenConfiguringConstructorDataSourcesInline
        : WhenConfiguringConstructorDataSourcesInline<Ef6TestDbContext>
    {
        public WhenConfiguringConstructorDataSourcesInline(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldErrorApplyingAConfiguredConstantByParameterTypeInline()
            => RunShouldErrorApplyingAConfiguredConstantByParameterTypeInline();
    }
}