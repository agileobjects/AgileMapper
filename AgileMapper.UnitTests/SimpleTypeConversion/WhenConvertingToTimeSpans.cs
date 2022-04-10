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
    public class WhenConvertingToTimeSpans
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableTimeSpanToATimeSpan()
        {
            var source = new PublicProperty<TimeSpan?> { Value = TimeSpan.FromHours(1) };
            var result = Mapper.Map(source).ToANew<PublicProperty<TimeSpan>>();

            result.Value.ShouldBe(TimeSpan.FromHours(1));
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAYearMonthDayStringToATimeSpan()
        {
            var source = new PublicProperty<string> { Value = "01:00:00" };
            var result = Mapper.Map(source).ToANew<PublicProperty<TimeSpan>>();

            result.Value.ShouldBe(TimeSpan.FromHours(01));
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparseableStringToANullableTimeSpan()
        {
            var source = new PublicProperty<string> { Value = "OH OH OH" };
            var result = Mapper.Map(source).ToANew<PublicProperty<TimeSpan?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullObjectStringToATimeSpan()
        {
            var source = new PublicProperty<object> { Value = default(string) };
            var result = Mapper.Map(source).ToANew<PublicProperty<TimeSpan>>();

            result.Value.ShouldBeDefault();
        }
    }
}
