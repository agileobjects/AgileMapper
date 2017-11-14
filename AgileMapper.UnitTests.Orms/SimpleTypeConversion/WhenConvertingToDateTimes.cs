﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
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

                context.StringItems.Add(new PublicString { Value = now.ToString("s") });
                context.SaveChanges();

                var dateTimeItem = context.StringItems.ProjectTo<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(now, TimeSpan.FromSeconds(1));
            }

            if (Context.StringToDateTimeConversionSupported)
            {
                RunTest(Test);
            }
            else
            {
                RunTestAndExpectThrow(Test);
            }
        }

        [Fact]
        public void ShouldProjectANullStringToADateTimeAsExpected()
        {
            void Test(TOrmContext context)
            {
                context.StringItems.Add(new PublicString { Value = default(string) });
                context.SaveChanges();

                var dateTimeItem = context.StringItems.ProjectTo<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(default(DateTime));
            }

            if (Context.StringToDateTimeConversionSupported)
            {
                RunTest(Test);
            }
            else
            {
                RunTestAndExpectThrow(Test);
            }
        }

        [Fact]
        public void ShouldProjectAnUnparseableStringToADateTimeAsExpected()
        {
            void Test(TOrmContext context)
            {
                context.StringItems.Add(new PublicString { Value = "htgijfoekld" });
                context.SaveChanges();

                var dateTimeItem = context.StringItems.ProjectTo<PublicDateTimeDto>().First();

                dateTimeItem.Value.ShouldBe(default(DateTime));
            }

            if (Context.StringToDateTimeValidationSupported)
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