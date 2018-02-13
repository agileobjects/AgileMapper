namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
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
        public Task ShouldProjectABoolToAString()
        {
            return RunTest(async context =>
            {
                await context.BoolItems.Add(new PublicBool { Value = true });
                await context.SaveChanges();

                var stringItem = context.BoolItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe("true");
            });
        }

        [Fact]
        public Task ShouldProjectAnIntToAString()
        {
            return RunTest(async context =>
            {
                await context.IntItems.Add(new PublicInt { Value = 763483 });
                await context.SaveChanges();

                var stringItem = context.IntItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe("763483");
            });
        }

        protected Task DoShouldProjectADateTimeToAString(Func<DateTime, string> expectedDateStringFactory)
        {
            return RunTest(async context =>
            {
                var now = DateTime.Now;

                await context.DateTimeItems.Add(new PublicDateTime { Value = now });
                await context.SaveChanges();

                var stringItem = context.DateTimeItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe(expectedDateStringFactory.Invoke(now));
            });
        }
    }
}
