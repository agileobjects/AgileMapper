namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreationInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringObjectCreationInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseAConditionalObjectFactoryInline()
        {
            return RunTest(async context =>
            {
                await context.IntItems.AddRangeAsync(
                    new PublicInt { Value = 1 },
                    new PublicInt { Value = 2 },
                    new PublicInt { Value = 3 });

                await context.SaveChangesAsync();

                var stringDtos = context
                    .IntItems
                    .OrderBy(p => p.Id)
                    .Project()
                    .To<PublicStringCtorDto>(cfg => cfg
                        .If(p => p.Value % 2 == 0)
                        .CreateInstancesUsing(p => new PublicStringCtorDto((p.Value * 2).ToString())))
                    .ToArray();

                stringDtos.First().Value.ShouldBe("1");
                stringDtos.Second().Value.ShouldBe("4");
                stringDtos.Third().Value.ShouldBe("3");
            });
        }
    }
}
