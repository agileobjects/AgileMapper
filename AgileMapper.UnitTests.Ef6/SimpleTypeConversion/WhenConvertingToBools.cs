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
        public void ShouldProjectAnIntOneToTrue()
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
        public void ShouldProjectAnIntZeroToFalse()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicIntProperty { Value = 0 });
                context.SaveChanges();

                var boolItem = context.IntItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringTrueToTrue()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "true" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringTrueToTrueIgnoringCase()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "tRuE" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringOneToTrue()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "1" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringFalseToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "false" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringZeroToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "0" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringNonBooleanValueToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "uokyujhygt" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringNullToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = null });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolPropertyDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }
    }
}
