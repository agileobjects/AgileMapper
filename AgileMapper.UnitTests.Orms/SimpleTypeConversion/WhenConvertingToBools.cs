namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToBools<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToBools(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAnIntOneToTrue()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicInt { Value = 1 });
                context.SaveChanges();

                var boolItem = context.IntItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAnIntZeroToFalse()
        {
            RunTest(context =>
            {
                context.IntItems.Add(new PublicInt { Value = 0 });
                context.SaveChanges();

                var boolItem = context.IntItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringTrueToTrue()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "true" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringTrueToTrueIgnoringCase()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "tRuE" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringOneToTrue()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "1" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProjectAStringFalseToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "false" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringZeroToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "0" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringNonBooleanValueToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = "uokyujhygt" });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public void ShouldProjectAStringNullToFalse()
        {
            RunTest(context =>
            {
                context.StringItems.Add(new PublicString { Value = null });
                context.SaveChanges();

                var boolItem = context.StringItems.ProjectTo<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }
    }
}
