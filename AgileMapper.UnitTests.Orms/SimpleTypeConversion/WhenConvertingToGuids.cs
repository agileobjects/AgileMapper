namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenConvertingToGuids<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToGuids(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAParseableStringToAGuidAsExpected()
        {
            void Test(TOrmContext context)
            {
                var guid = Guid.NewGuid();

                context.StringItems.Add(new PublicString { Value = guid.ToString() });
                context.SaveChanges();

                var guidItem = context.StringItems.ProjectTo<PublicGuidDto>().First();

                guidItem.Value.ShouldBe(guid);
            }

            if (Context.StringToGuidConversionSupported)
            {
                RunTest(Test);
            }
            else
            {
                RunTestAndExpectThrow(Test);
            }
        }

        [Fact]
        public void ShouldProjectANullStringToAGuidAsExpected()
        {
            void Test(TOrmContext context)
            {
                context.StringItems.Add(new PublicString { Value = default(string) });
                context.SaveChanges();

                var guidItem = context.StringItems.ProjectTo<PublicGuidDto>().First();

                guidItem.Value.ShouldBe(default(Guid));
            }

            if (Context.StringToGuidConversionSupported)
            {
                RunTest(Test);
            }
            else
            {
                RunTestAndExpectThrow(Test);
            }
        }
    }
}
