namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringObjectCreation<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringObjectCreation(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseACustomObjectFactory()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "Ctor!" });
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ProjectionsTo<PublicStringCtorDto>()
                        .CreateInstancesUsing(o => new PublicStringCtorDto("PANTS"));

                    var ctorDto = context
                        .StringItems
                        .ProjectUsing(mapper)
                        .To<PublicStringCtorDto>()
                        .ShouldHaveSingleItem();

                    ctorDto.Value.ShouldBe("PANTS");
                }
            });
        }
    }
}
