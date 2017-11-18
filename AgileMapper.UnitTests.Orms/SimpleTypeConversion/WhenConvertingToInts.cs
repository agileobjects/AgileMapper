namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
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
        public void ShouldProjectAShortToAnInt()
        {
            RunTest(context =>
            {
                context.ShortItems.Add(new PublicShort { Value = 123 });
                context.SaveChanges();

                var intItem = context.ShortItems.ProjectTo<PublicIntDto>().First();

                intItem.Value.ShouldBe(123);
            });
        }

        [Fact]
        public void ShouldProjectAnInRangeLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLong { Value = 12345L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntDto>().First();

                intItem.Value.ShouldBe(12345);
            });
        }

        [Fact]
        public void ShouldProjectATooBigLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLong { Value = long.MaxValue });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        [Fact]
        public void ShouldProjectATooSmallLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLong { Value = int.MinValue - 1L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        #region Parseable String -> Int

        protected void RunShouldProjectAParseableStringToAnInt()
            => RunTest(ProjectParseableStringToInt);

        protected void RunShouldErrorProjectingAParseableStringToAnInt()
            => RunTestAndExpectThrow(ProjectParseableStringToInt);

        private static void ProjectParseableStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "738" });
            context.SaveChanges();

            var intItem = context.StringItems.ProjectTo<PublicIntDto>().First();

            intItem.Value.ShouldBe(738);
        }

        #endregion

        #region Null String -> Int

        protected void RunShouldProjectANullStringToAnInt()
            => RunTest(ProjectNullStringToInt);

        protected void RunShouldErrorProjectingANullStringToAnInt()
            => RunTestAndExpectThrow(ProjectNullStringToInt);

        private static void ProjectNullStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            context.SaveChanges();

            var intItem = context.StringItems.ProjectTo<PublicIntDto>().First();

            intItem.Value.ShouldBe(default(int));
        }

        #endregion

        #region Unparseable String -> Int

        protected void RunShouldProjectAnUnparseableStringToAnInt()
            => RunTest(ProjectUnparseableStringToInt);

        protected void RunShouldErrorProjectingAnUnparseableStringToAnInt()
            => RunTestAndExpectThrow(ProjectUnparseableStringToInt);

        private static void ProjectUnparseableStringToInt(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "hsejk" });
            context.SaveChanges();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            context.StringItems.ProjectTo<PublicIntDto>().First();
        }

        #endregion
    }
}