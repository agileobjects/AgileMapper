namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
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

        protected void RunShouldProjectAParseableStringToADateTime()
            => RunTest(ProjectParseableStringToADateTime);

        protected void RunShouldErrorProjectingAParseableStringToADateTime()
            => RunTestAndExpectThrow(ProjectParseableStringToADateTime);

        private static void ProjectParseableStringToADateTime(TOrmContext context)
        {
            var now = DateTime.Now;

            context.StringItems.Add(new PublicString { Value = now.ToString("s") });
            context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
        }

        #endregion

        #region Null String -> DateTime

        protected void RunShouldProjectANullStringToADateTime()
            => RunTest(ProjectANullStringToADateTime);

        protected void RunShouldErrorProjectingANullStringToADateTime()
            => RunTestAndExpectThrow(ProjectANullStringToADateTime);

        private static void ProjectANullStringToADateTime(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(default(DateTime));
        }

        #endregion

        #region Unparseable String -> DateTime

        protected void RunShouldProjectAnUnparseableStringToADateTime()
            => RunTest(ProjectAnUnparseableStringToADateTime);

        protected void RunShouldErrorProjectingAnUnparseableStringToADateTime()
            => RunTestAndExpectThrow(ProjectAnUnparseableStringToADateTime);

        private static void ProjectAnUnparseableStringToADateTime(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "htgijfoekld" });
            context.SaveChanges();

            var dateTimeItem = context.StringItems.Project().To<PublicDateTimeDto>().First();

            dateTimeItem.Value.ShouldBe(default(DateTime));
        }

        #endregion
    }
}
