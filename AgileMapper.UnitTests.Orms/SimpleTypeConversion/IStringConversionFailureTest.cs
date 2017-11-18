namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    // ReSharper disable once UnusedTypeParameter
    public interface IStringConversionFailureTest<TTarget>
    {
        void ShouldErrorProjectingAParseableString();

        void ShouldErrorProjectingANullString();
    }
}