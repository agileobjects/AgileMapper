namespace AgileObjects.AgileMapper.UnitTests.Ef6.SimpleTypeConversion
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToBools : Ef6TestClassBase
    {
        public WhenConvertingToBools(TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldMapAnIntOneToTrue()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicIntProperty { Value = 1 });
                context.SaveChanges();

                var boolItem = context.IntItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldMapAnIntZeroToFalse()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicIntProperty { Value = 0 });
                context.SaveChanges();

                var boolItem = context.IntItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }
    }
}
