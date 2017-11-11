namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToDateTimes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToDateTimes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableStringToADateTimeAsExpected()
        {
            void Test(TOrmContext context)
            {
                var now = DateTime.Now;
                var nowString = now.ToString(CultureInfo.InvariantCulture);

                context.StringItems.Add(new PublicString { Value = nowString });
                context.SaveChanges();

                var dateTimeItem = context.StringItems.ProjectTo<PublicDateTimeDto>().First();

                dateTimeItem.Value.ToString(CultureInfo.InvariantCulture).ShouldBe(nowString);
            }

            RunTest(Test);
        }
    }
}
