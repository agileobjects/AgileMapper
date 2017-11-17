namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion.Guids
{
    public interface IGuidConverterTest
    {
        void ShouldProjectAParseableStringToAGuid();

        void ShouldProjectANullStringToAGuid();
    }
}