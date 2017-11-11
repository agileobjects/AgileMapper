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
        protected WhenConvertingToInts(ITestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAShortToAnInt()
        {
            RunTest(context =>
            {
                context.ShortItems.Add(new PublicShortProperty { Value = 123 });
                context.SaveChanges();

                var intItem = context.ShortItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(123);
            });
        }

        [Fact]
        public void ShouldProjectAnInRangeLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = 12345L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(12345);
            });
        }

        [Fact]
        public void ShouldProjectATooBigLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = long.MaxValue });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        [Fact]
        public void ShouldProjectATooSmallLongToAnInt()
        {
            RunTest(context =>
            {
                context.LongItems.Add(new PublicLongProperty { Value = int.MinValue - 1L });
                context.SaveChanges();

                var intItem = context.LongItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(0);
            });
        }

        [Fact]
        public void ShouldProjectAParseableStringToAnIntAsExpected()
        {
            void Test(TOrmContext context)
            {
                context.StringItems.Add(new PublicStringProperty { Value = "738" });
                context.SaveChanges();

                var intItem = context.StringItems.ProjectTo<PublicIntPropertyDto>().First();

                intItem.Value.ShouldBe(738);
            }

            if (Context.StringParsingSupported)
            {
                RunTest(Test);
            }
            else
            {
                RunTestAndExpectThrow(Test);
            }
        }

        [Fact]
        public void ShouldProjectAnUnparseableStringToAnIntAsExpected()
        {
            RunTestAndExpectThrow(context =>
            {
                context.StringItems.Add(new PublicStringProperty { Value = "hsejk" });
                context.SaveChanges();

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.StringItems.ProjectTo<PublicIntPropertyDto>().First();
            });
        }
    }
}