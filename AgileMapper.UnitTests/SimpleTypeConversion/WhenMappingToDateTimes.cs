namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenMappingToDateTimes
    {
        [Fact]
        public void ShouldMapANullableDateTimeToADateTime()
        {
            var source = new PublicProperty<DateTime?> { Value = DateTime.Today };
            var result = Mapper.Map(source).ToNew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(DateTime.Today);
        }

        [Fact]
        public void ShouldMapAYearMonthDayStringToADateTime()
        {
            var source = new PublicProperty<string> { Value = "2016/06/08" };
            var result = Mapper.Map(source).ToNew<PublicProperty<DateTime>>();

            result.Value.ShouldBe(new DateTime(2016, 06, 08));
        }
    }
}
