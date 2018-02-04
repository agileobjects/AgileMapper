namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToDoubles<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToDoubles(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAShortToADouble()
        {
            return RunTest(async context =>
            {
                context.ShortItems.Add(new PublicShort { Value = 123 });
                await context.SaveChanges();

                var doubleItem = context.ShortItems.Project().To<PublicDoubleDto>().First();

                doubleItem.Value.ShouldBe(123d);
            });
        }

        [Fact]
        public Task ShouldProjectALongToADouble()
        {
            return RunTest(async context =>
            {
                context.LongItems.Add(new PublicLong { Value = 12345L });
                await context.SaveChanges();

                var doubleItem = context.LongItems.Project().To<PublicDoubleDto>().First();

                doubleItem.Value.ShouldBe(12345d);
            });
        }

        #region Parseable String -> Double

        protected Task RunShouldProjectAParseableStringToADouble()
            => RunTest(ProjectParseableStringToDouble);

        protected Task RunShouldErrorProjectingAParseableStringToADouble()
            => RunTestAndExpectThrow(ProjectParseableStringToDouble);

        private static async Task ProjectParseableStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "738.01" });
            await context.SaveChanges();

            var doubleItem = context.StringItems.Project().To<PublicDoubleDto>().First();

            doubleItem.Value.ShouldBe(738.01);
        }

        #endregion

        #region Null String -> Double

        protected Task RunShouldProjectANullStringToADouble()
            => RunTest(ProjectNullStringToDouble);

        protected Task RunShouldErrorProjectingANullStringToADouble()
            => RunTestAndExpectThrow(ProjectNullStringToDouble);

        private static async Task ProjectNullStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            await context.SaveChanges();

            var doubleItem = context.StringItems.Project().To<PublicDoubleDto>().First();

            doubleItem.Value.ShouldBe(default(double));
        }

        #endregion

        #region Unparseable String -> Double

        protected Task RunShouldProjectAnUnparseableStringToADouble()
            => RunTest(ProjectUnparseableStringToDouble);

        protected Task RunShouldErrorProjectingAnUnparseableStringToADouble()
            => RunTestAndExpectThrow(ProjectUnparseableStringToDouble);

        private static async Task ProjectUnparseableStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "poioiujygy" });
            await context.SaveChanges();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            context.StringItems.Project().To<PublicDoubleDto>().First();
        }

        #endregion
    }
}