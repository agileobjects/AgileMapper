namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectTracking : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringObjectTracking(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldIgnoreObjectTracking()
        {
            return RunTest(async (context, mapper) =>
            {
                var circle1 = new Circle { Diameter = 1 };
                var circle2 = new Circle { Diameter = 2 };
                var circle3 = new Circle { Diameter = 3 };

                await context.Shapes.AddRangeAsync(circle1, circle2, circle3);
                await context.SaveChangesAsync();

                mapper.WhenMapping
                    .To<CircleViewModel>()
                    .MaintainIdentityIntegrity();

                var circleVms = await context
                    .Shapes
                    .ProjectUsing(mapper)
                    .To<Circle>()
                    .OrderBy(c => c.Diameter)
                    .ToArrayAsync();

                circleVms.Length.ShouldBe(3);

                circleVms.First().Diameter.ShouldBe(1);
                circleVms.Second().Diameter.ShouldBe(2);
                circleVms.Third().Diameter.ShouldBe(3);
            });
        }
    }
}
