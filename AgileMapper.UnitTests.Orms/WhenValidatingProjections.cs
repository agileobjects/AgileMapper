namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenValidatingProjections<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenValidatingProjections(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldSupportCachedProjectionValidation()
        {
            return RunTest(context =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlanForProjecting(context.Addresses).To<AddressDto>();

                    Should.NotThrow(() => mapper.ThrowNowIfAnyMappingPlanIsIncomplete());
                }

                return Task.CompletedTask;
            });
        }
    }
}
