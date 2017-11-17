namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion.Guids
{
    public interface IGuidConversionFailureTest
    {
        void ShouldErrorProjectingAParseableStringToAGuid();

        void ShouldErrorProjectingANullStringToAGuid();
    }
}