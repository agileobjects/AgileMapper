namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToDateTimes
    {
        [Fact]
        public void ShouldMapANullableDateTimeToADateTime()
        {
            var source = new PublicProperty<DateTime?> { Value = DateTime.Today };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(DateTime.Today);
        }

        [Fact]
        public void ShouldMapAYearMonthDayStringToADateTime()
        {
            var source = new PublicProperty<string> { Value = "2016/06/08" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(new DateTime(2016, 06, 08));
        }

        [Fact]
        public void ShouldMapAnUnparseableStringToANullableDateTime()
        {
            var source = new PublicProperty<string> { Value = "OOH OOH OOH" };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapANullObjectStringToADateTime()
        {
            var source = new PublicProperty<object> { Value = default(string) };
            var result = Mapper.Map(source).ToANew<PublicProperty<DateTime>>();

            result.Value.ShouldBeDefault();
        }
    }
}
