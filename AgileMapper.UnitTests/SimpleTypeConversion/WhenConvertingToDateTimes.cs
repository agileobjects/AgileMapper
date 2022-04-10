namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using Common;
    using Common.TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToDateTimes
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableDateTimeToADateTime()
        {
            var source = new PublicProperty<DateTime?> { Value = DateTime.Today };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(DateTime.Today);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAYearMonthDayStringToADateTime()
        {
            var source = new PublicProperty<string> { Value = "2016/06/08" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(new DateTime(2016, 06, 08));
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparseableStringToANullableDateTime()
        {
            var source = new PublicProperty<string> { Value = "OOH OOH OOH" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullObjectStringToADateTime()
        {
            var source = new PublicProperty<object> { Value = default(string) };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBeDefault();
        }
    }
}
