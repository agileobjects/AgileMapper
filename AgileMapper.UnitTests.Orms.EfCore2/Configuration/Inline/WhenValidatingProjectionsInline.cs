namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Validation;
    using Xunit;

    public class WhenValidatingProjectionsInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenValidatingProjectionsInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldSupportCachedProjectionValidationInline()
        {
            return RunTest(async context =>
            {
                var address = new Address { Line1 = "1" };

                await context.Addresses.AddAsync(address);
                await context.SaveChangesAsync();

                Should.NotThrow(async () =>
                {
                    var addressDto = await context
                        .Addresses
                        .Project().To<AddressDto>(cfg => cfg
                            .ThrowNowIfMappingPlanIsIncomplete())
                        .FirstAsync();

                    addressDto.Line1.ShouldBe("1");
                });
            });
        }

        [Fact]
        public Task ShouldErrorIfCachedProjectionTargetTypeIsUnconstructableInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var validationEx = await Should.ThrowAsync<MappingValidationException>(async () =>
                {
                    await context
                        .Addresses
                        .ProjectUsing(mapper)
                        .To<ProductStruct>(cfg => cfg
                            .ThrowNowIfMappingPlanIsIncomplete())
                        .FirstOrDefaultAsync();
                });

                validationEx.Message.ShouldContain("IQueryable<Address> -> IQueryable<ProductStruct>");
                validationEx.Message.ShouldContain("Rule set: Project");
                validationEx.Message.ShouldContain("Unconstructable target Types");
                validationEx.Message.ShouldContain("Address -> ProductStruct");
            });
        }
    }
}
