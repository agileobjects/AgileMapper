namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToStrings<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToStrings(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAnIntToAString()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicIntProperty { Value = 763483 });
                context.SaveChanges();

                var stringItem = context.IntItems.ProjectTo<PublicStringPropertyDto>().First();

                stringItem.Value.ShouldBe("763483");
            });
        }

        [Fact]
        public void ShouldProjectABoolToAString()
        {
            RunTest(context =>
            {
                context.BoolItems.Add(new PublicBoolProperty { Value = true });
                context.SaveChanges();

                var stringItem = context.BoolItems.ProjectTo<PublicStringPropertyDto>().First();

                stringItem.Value.ShouldBe("true");
            });
        }
    }
}
