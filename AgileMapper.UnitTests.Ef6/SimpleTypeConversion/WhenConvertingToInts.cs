namespace AgileObjects.AgileMapper.UnitTests.Ef6.SimpleTypeConversion
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToInts : Ef6TestClassBase
    {
        public WhenConvertingToInts(TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldMapAShortToAnInt()
        {
            RunTest(context =>
            {
                context.ShortItems.Add(new PublicShortProperty { Value = 123 });
                context.SaveChanges();

                var intItem = context.ShortItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(123);
            });
        }
    }
}