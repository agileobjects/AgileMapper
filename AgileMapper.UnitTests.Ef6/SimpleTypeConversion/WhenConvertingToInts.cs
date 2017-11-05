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
        public void ShouldProjectAShortToAnInt()
        {
            RunTest(context =>
            {
                context.ShortItems.Add(new PublicShortProperty { Value = 123 });
                context.SaveChanges();

                var intItem = context.ShortItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(123);
            });
        }

        [Fact]
        public void ShouldProjectAnInRangeLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = 12345L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(12345);
            });
        }

        [Fact]
        public void ShouldProjectATooBigLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = long.MaxValue });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        [Fact]
        public void ShouldProjectATooSmallLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = int.MinValue - 1L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }
    }
}