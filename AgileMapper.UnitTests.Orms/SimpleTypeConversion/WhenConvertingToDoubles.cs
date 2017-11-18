namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
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
        public void ShouldProjectAShortToADouble()
        {
            RunTest(context =>
            {
                context.ShortItems.Add(new PublicShort { Value = 123 });
                context.SaveChanges();

                var doubleItem = context.ShortItems.ProjectTo<PublicDoubleDto>().First();

                doubleItem.Value.ShouldBe(123d);
            });
        }

        [Fact]
        public void ShouldProjectALongToADouble()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLong { Value = 12345L });
                context.SaveChanges();

                var doubleItem = context.LongItems.ProjectTo<PublicDoubleDto>().First();

                doubleItem.Value.ShouldBe(12345d);
            });
        }

        #region Parseable String -> Double

        protected void RunShouldProjectAParseableStringToADouble()
            => RunTest(ProjectParseableStringToDouble);

        protected void RunShouldErrorProjectingAParseableStringToADouble()
            => RunTestAndExpectThrow(ProjectParseableStringToDouble);

        private static void ProjectParseableStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "738.01" });
            context.SaveChanges();

            var doubleItem = context.StringItems.ProjectTo<PublicDoubleDto>().First();

            doubleItem.Value.ShouldBe(738.01);
        }

        #endregion

        #region Null String -> Double

        protected void RunShouldProjectANullStringToADouble()
            => RunTest(ProjectNullStringToDouble);

        protected void RunShouldErrorProjectingANullStringToADouble()
            => RunTestAndExpectThrow(ProjectNullStringToDouble);

        private static void ProjectNullStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            context.SaveChanges();

            var doubleItem = context.StringItems.ProjectTo<PublicDoubleDto>().First();

            doubleItem.Value.ShouldBe(default(double));
        }

        #endregion

        #region Unparseable String -> Double

        protected void RunShouldProjectAnUnparseableStringToADouble()
            => RunTest(ProjectUnparseableStringToDouble);

        protected void RunShouldErrorProjectingAnUnparseableStringToADouble()
            => RunTestAndExpectThrow(ProjectUnparseableStringToDouble);

        private static void ProjectUnparseableStringToDouble(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = "poioiujygy" });
            context.SaveChanges();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            context.StringItems.ProjectTo<PublicDoubleDto>().First();
        }

        #endregion
    }
}