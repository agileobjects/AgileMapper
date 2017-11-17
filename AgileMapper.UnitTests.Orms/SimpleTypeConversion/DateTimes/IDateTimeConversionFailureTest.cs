namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion.DateTimes
{
    public interface IDateTimeConversionFailureTest
    {
        void ShouldErrorProjectingAParseableStringToADateTime();

        void ShouldErrorProjectingANullStringToADateTime();
    }
}