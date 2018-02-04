namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToInts<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToInts(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAShortToAnInt()
        {
            return RunTest(async context =>
            {
                context.ShortItems.Add(new PublicShort { Value = 123 });
                await context.SaveChanges();

                var intItem = context.ShortItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(123);
            });
        }

        [Fact]
        public Task ShouldProjectAnInRangeLongToAnInt()
        {
            return RunTest(async context =>
            {
                context.LongItems.Add(new PublicLong { Value = 12345L });
                await context.SaveChanges();

                var intItem = context.LongItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(12345);
            });
        }

        [Fact]
        public Task ShouldProjectATooBigLongToAnInt()
        {
            return RunTest(async context =>
            {
                context.LongItems.Add(new PublicLong { Value = long.MaxValue });
                await context.SaveChanges();

                var intItem = context.LongItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        [Fact]
        public Task ShouldProjectATooSmallLongToAnInt()
        {
            return RunTest(async context =>
            {
                context.LongItems.Add(new PublicLong { Value = int.MinValue - 1L });
                await context.SaveChanges();

                var intItem = context.LongItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        #region Parseable String -> Int

        protected Task RunShouldProjectAParseableStringToAnInt()
            => RunTest(ProjectParseableStringToInt);

        protected Task RunShouldErrorProjectingAParseableStringToAnInt()
            => RunTestAndExpectThrow(ProjectParseableStringToInt);

        private static async Task ProjectParseableStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "738" });
            await context.SaveChanges();

            var intItem = context.StringItems.Project().To<PublicIntDto>().First();

            intItem.Value.ShouldBe(738);
        }

        #endregion

        #region Null String -> Int

        protected Task RunShouldProjectANullStringToAnInt()
            => RunTest(ProjectNullStringToInt);

        protected Task RunShouldErrorProjectingANullStringToAnInt()
            => RunTestAndExpectThrow(ProjectNullStringToInt);

        private static async Task ProjectNullStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            await context.SaveChanges();

            var intItem = context.StringItems.Project().To<PublicIntDto>().First();

            intItem.Value.ShouldBe(default(int));
        }

        #endregion

        #region Unparseable String -> Int

        protected Task RunShouldProjectAnUnparseableStringToAnInt()
            => RunTest(ProjectUnparseableStringToInt);

        protected Task RunShouldErrorProjectingAnUnparseableStringToAnInt()
            => RunTestAndExpectThrow(ProjectUnparseableStringToInt);

        private static async Task ProjectUnparseableStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "hsejk" });
            await context.SaveChanges();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            context.StringItems.Project().To<PublicIntDto>().First();
        }

        #endregion
    }
}