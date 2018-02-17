namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using UnitTests.TestClasses;
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

        [Fact]
        public Task ShouldProjectAnNullNullableIntToAString()
        {
            return RunTest(async context =>
            {
                await context.NullableIntItems.Add(new PublicNullableInt { Value = default(int?) });
                await context.SaveChanges();

                var stringItem = context.NullableIntItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBeNull();
            });
        }

        protected Task DoShouldProjectAnEnumToAString(Func<Title, string> expectedEnumStringFactory = null)
        {
            if (expectedEnumStringFactory == null)
            {
                expectedEnumStringFactory = t => t.ToString();
            }

            return RunTest(async context =>
            {
                await context.TitleItems.Add(new PublicTitle { Value = Title.Dr });
                await context.SaveChanges();

                var stringItem = context.TitleItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe(expectedEnumStringFactory.Invoke(Title.Dr));
            });
        }

        protected Task DoShouldProjectADecimalToAString(Func<decimal, string> expectedDecimalStringFactory = null)
        {
            if (expectedDecimalStringFactory == null)
            {
                expectedDecimalStringFactory = d => d + "";
            }

            return RunTest(async context =>
            {
                await context.DecimalItems.Add(new PublicDecimal { Value = 728.261m });
                await context.SaveChanges();

                var stringItem = context.DecimalItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe(expectedDecimalStringFactory.Invoke(728.261m));
            });
        }

        protected Task DoShouldProjectADoubleToAString(Func<double, string> expectedDoubleStringFactory = null)
        {
            if (expectedDoubleStringFactory == null)
            {
                expectedDoubleStringFactory = d => d + "";
            }

            return RunTest(async context =>
            {
                await context.DoubleItems.Add(new PublicDouble { Value = 7212.34 });
                await context.SaveChanges();

                var stringItem = context.DoubleItems.Project().To<PublicStringDto>().First();

                stringItem.Value.ShouldBe(expectedDoubleStringFactory.Invoke(7212.34));
            });
        }

        protected Task DoShouldProjectADateTimeToAString(Func<DateTime, string> expectedDateStringFactory = null)
        {
            if (expectedDateStringFactory == null)
            {
                expectedDateStringFactory = d => d.ToString(CultureInfo.CurrentCulture.DateTimeFormat);
            }

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
