namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion.DateTimes
{
    public interface IDateTimeConverterTest
    {
        void ShouldProjectAParseableStringToADateTime();

        void ShouldProjectANullStringToADateTime();
    }
}