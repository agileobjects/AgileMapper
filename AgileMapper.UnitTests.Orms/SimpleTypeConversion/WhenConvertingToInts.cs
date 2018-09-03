namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
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
                await context.ShortItems.Add(new PublicShort { Value = 123 });
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
                await context.LongItems.Add(new PublicLong { Value = 12345L });
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
                await context.LongItems.Add(new PublicLong { Value = long.MaxValue });
                await context.SaveChanges();

                var intItem = context.LongItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectATooSmallLongToAnInt()
        {
            return RunTest(async context =>
            {
                await context.LongItems.Add(new PublicLong { Value = int.MinValue - 1L });
                await context.SaveChanges();

                var intItem = context.LongItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectAnInRangeDecimalToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DecimalItems.Add(new PublicDecimal { Value = 73872 });
                await context.SaveChanges();

                var intItem = context.DecimalItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(73872);
            });
        }

        [Fact]
        public Task ShouldProjectATooBigDecimalToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DecimalItems.Add(new PublicDecimal { Value = decimal.MaxValue });
                await context.SaveChanges();

                var intItem = context.DecimalItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectATooSmallDecimalToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DecimalItems.Add(new PublicDecimal { Value = int.MinValue - 1.0m });
                await context.SaveChanges();

                var intItem = context.DecimalItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectANonWholeNumberDecimalToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DecimalItems.Add(new PublicDecimal { Value = 829.26m });
                await context.SaveChanges();

                var intItem = context.DecimalItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectAnInRangeDoubleToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DoubleItems.Add(new PublicDouble { Value = 7382.00 });
                await context.SaveChanges();

                var intItem = context.DoubleItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBe(7382);
            });
        }

        [Fact]
        public Task ShouldProjectATooBigDoubleToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DoubleItems.Add(new PublicDouble { Value = double.MaxValue });
                await context.SaveChanges();

                var intItem = context.DoubleItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectATooSmallDoubleToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DoubleItems.Add(new PublicDouble { Value = int.MinValue - 1.00 });
                await context.SaveChanges();

                var intItem = context.DoubleItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        [Fact]
        public Task ShouldProjectANonWholeNumberDoubleToAnInt()
        {
            return RunTest(async context =>
            {
                await context.DoubleItems.Add(new PublicDouble { Value = 82.271 });
                await context.SaveChanges();

                var intItem = context.DoubleItems.Project().To<PublicIntDto>().First();

                intItem.Value.ShouldBeDefault();
            });
        }

        #region Parseable String -> Int

        protected Task RunShouldProjectAParseableStringToAnInt()
            => RunTest(ProjectParseableStringToInt);

        protected Task RunShouldErrorProjectingAParseableStringToAnInt()
            => RunTestAndExpectThrow(ProjectParseableStringToInt);

        private static async Task ProjectParseableStringToInt(TOrmContext context)
        {
            await context.StringItems.Add(new PublicString { Value = "738" });
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
            await context.StringItems.Add(new PublicString { Value = default(string) });
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
            await context.StringItems.Add(new PublicString { Value = "hsejk" });
            await context.SaveChanges();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            context.StringItems.Project().To<PublicIntDto>().First();
        }

        #endregion
    }
}