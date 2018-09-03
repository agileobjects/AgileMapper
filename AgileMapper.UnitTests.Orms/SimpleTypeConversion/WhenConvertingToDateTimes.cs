namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConvertingToDateTimes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToDateTimes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        protected Task DoShouldProjectANullableDateTimeToADateTime()
        {
            return RunTest(async context =>
            {
                var now = DateTime.Now;

                await context.NullableDateTimeItems.Add(new PublicNullableDateTime { Value = now });
                await context.SaveChanges();

                var dateTimeItem = context.NullableDateTimeItems.Project().To<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
            });
        }

        protected Task DoShouldProjectANullNullableDateTimeToADateTime()
        {
            return RunTest(async context =>
            {
                await context.NullableDateTimeItems.Add(new PublicNullableDateTime { Value = default(DateTime?) });
                await context.SaveChanges();

                var dateTimeItem = context.NullableDateTimeItems.Project().To<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBeDefault();
            });
        }

        protected Task DoShouldProjectAParseableStringToADateTime()
        {
            return RunTest(async context =>
            {
                var now = DateTime.Now;

                await context.StringItems.Add(new PublicString { Value = now.ToString("s") });
                await context.SaveChanges();

                var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
            });
        }

        protected Task DoShouldProjectANullStringToADateTime()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = default(string) });
                await context.SaveChanges();

                var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(default(DateTime));
            });
        }

        #region Unparseable String -> DateTime

        protected Task RunShouldProjectAnUnparseableStringToADateTime()
            => RunTest(ProjectAnUnparseableStringToADateTime);

        protected Task RunShouldErrorProjectingAnUnparseableStringToADateTime()
            => RunTestAndExpectThrow(ProjectAnUnparseableStringToADateTime);

        private static async Task ProjectAnUnparseableStringToADateTime(TOrmContext context)
        {
            await context.StringItems.Add(new PublicString { Value = "htgijfoekld" });
            await context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(default(DateTime));
        }

        #endregion
    }
}
