namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using UnitTests.TestClasses;
    using Xunit;
    using static UnitTests.TestClasses.Title;

    public abstract class WhenConvertingToEnums<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToEnums(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAByteToAnEnum()
        {
            return RunTest(async context =>
            {
                context.ByteItems.Add(new PublicByte { Value = (byte)Dr });
                await context.SaveChanges();

                var enumItem = context.ByteItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Dr);
            });
        }

        [Fact]
        public Task ShouldProjectAShortToAnEnum()
        {
            return RunTest(async context =>
            {
                context.ShortItems.Add(new PublicShort { Value = (short)Count });
                await context.SaveChanges();

                var enumItem = context.ShortItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Count);
            });
        }

        [Fact]
        public Task ShouldProjectAnIntToAnEnum()
        {
            return RunTest(async context =>
            {
                context.IntItems.Add(new PublicInt { Value = (int)Duke });
                await context.SaveChanges();

                var enumItem = context.IntItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Duke);
            });
        }

        [Fact]
        public Task ShouldProjectALongToAnEnum()
        {
            return RunTest(async context =>
            {
                context.LongItems.Add(new PublicLong { Value = (long)Ms });
                await context.SaveChanges();

                var enumItem = context.LongItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Ms);
            });
        }

        [Fact]
        public Task ShouldProjectAMatchingStringToAnEnum()
        {
            return RunTest(async context =>
            {
                context.StringItems.Add(new PublicString { Value = Mr.ToString() });
                await context.SaveChanges();

                var enumItem = context.StringItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Mr);
            });
        }

        [Fact]
        public Task ShouldProjectAMatchingNumericStringToAnEnum()
        {
            return RunTest(async context =>
            {
                context.StringItems.Add(new PublicString { Value = ((int)Mrs).ToString() });
                await context.SaveChanges();

                var enumItem = context.StringItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(Mrs);
            });
        }

        [Fact]
        public Task ShouldProjectANonMatchingStringToAnEnum()
        {
            return RunTest(async context =>
            {
                context.StringItems.Add(new PublicString { Value = "Horse Pills" });
                await context.SaveChanges();

                var enumItem = context.StringItems.Project().To<PublicTitleDto>().ShouldHaveSingleItem();

                enumItem.Value.ShouldBe(default(Title));
            });
        }
    }
}
