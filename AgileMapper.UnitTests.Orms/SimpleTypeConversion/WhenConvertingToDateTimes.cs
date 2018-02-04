namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConvertingToDateTimes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToDateTimes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Parseable String -> DateTime

        protected Task RunShouldProjectAParseableStringToADateTime()
            => RunTest(ProjectParseableStringToADateTime);

        protected Task RunShouldErrorProjectingAParseableStringToADateTime()
            => RunTestAndExpectThrow(ProjectParseableStringToADateTime);

        private static async Task ProjectParseableStringToADateTime(TOrmContext context)
        {
            var now = DateTime.Now;

            context.StringItems.Add(new PublicString { Value = now.ToString("s") });
            await context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
        }

        #endregion

        #region Null String -> DateTime

        protected Task RunShouldProjectANullStringToADateTime()
            => RunTest(ProjectANullStringToADateTime);

        protected Task RunShouldErrorProjectingANullStringToADateTime()
            => RunTestAndExpectThrow(ProjectANullStringToADateTime);

        private static async Task ProjectANullStringToADateTime(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            await context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(default(DateTime));
        }

        #endregion

        #region Unparseable String -> DateTime

        protected Task RunShouldProjectAnUnparseableStringToADateTime()
            => RunTest(ProjectAnUnparseableStringToADateTime);

        protected Task RunShouldErrorProjectingAnUnparseableStringToADateTime()
            => RunTestAndExpectThrow(ProjectAnUnparseableStringToADateTime);

        private static async Task ProjectAnUnparseableStringToADateTime(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "htgijfoekld" });
            await context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(default(DateTime));
        }

        #endregion
    }
}
