namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreation : WhenConfiguringObjectCreation<EfCore2TestDbContext>
    {
        public WhenConfiguringObjectCreation(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseAConditionalObjectFactory() => RunShouldUseAConditionalObjectFactory();

        [Fact]
        public Task ShouldNotUseMappingDataConfiguredSourceAndTargetDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .CreateInstancesUsing(ctx => new PublicIntDto { Value = ctx.ElementIndex.GetValueOrDefault() });

                await context.IntItems.AddAsync(new PublicInt { Value = 17 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(17);
            });
        }
    }
}
