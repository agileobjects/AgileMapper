namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
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
        public Task ShouldProjectAnIntOneToTrue()
        {
            return RunTest(async context =>
            {
                await context.IntItems.Add(new PublicInt { Value = 1 });
                await context.SaveChanges();

                var boolItem = context.IntItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldProjectAnIntZeroToFalse()
        {
            return RunTest(async context =>
            {
                await context.IntItems.Add(new PublicInt { Value = 0 });
                await context.SaveChanges();

                var boolItem = context.IntItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public Task ShouldProjectAStringTrueToTrue()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "true" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldProjectAStringTrueToTrueIgnoringCase()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "tRuE" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldProjectAStringOneToTrue()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "1" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeTrue();
            });
        }

        [Fact]
        public Task ShouldProjectAStringFalseToFalse()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "false" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public Task ShouldProjectAStringZeroToFalse()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "0" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public Task ShouldProjectAStringNonBooleanValueToFalse()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "uokyujhygt" });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }

        [Fact]
        public Task ShouldProjectAStringNullToFalse()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = null });
                await context.SaveChanges();

                var boolItem = context.StringItems.Project().To<PublicBoolDto>().First();

                boolItem.Value.ShouldBeFalse();
            });
        }
    }
}
