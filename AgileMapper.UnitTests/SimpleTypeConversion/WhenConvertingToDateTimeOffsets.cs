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
    public class WhenConvertingToDateTimeOffsets
    {
        [Fact]
        public void ShouldMapANullableDateTimeOffsetToADateTimeOffset()
        {
            var source = new PublicProperty<DateTimeOffset?> { Value = DateTime.Today.AddHours(12) };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTimeOffset>>();

            result.Value.ShouldBe(new DateTimeOffset(DateTime.Today.AddHours(12)));
        }

        [Fact]
        public void ShouldMapAYearMonthDayStringToADateTimeOffset()
        {
            var source = new PublicProperty<string> { Value = "2020/01/05" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTimeOffset>>();

            result.Value.ShouldBe(new DateTimeOffset(new DateTime(2020, 01, 05)));
        }

        [Fact]
        public void ShouldMapAnUnparseableStringToANullableDateTimeOffset()
        {
            var source = new PublicProperty<string> { Value = "NO NO NO" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTimeOffset?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapANullObjectStringToADateTimeOffset()
        {
            var source = new PublicProperty<object> { Value = default(string) };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTimeOffset>>();

            result.Value.ShouldBeDefault();
        }
    }
}
