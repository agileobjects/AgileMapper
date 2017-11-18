namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    public interface IStringConversionFailureTest
    {
        void ShouldErrorProjectingAParseableString();

        void ShouldErrorProjectingANullString();
    }
}