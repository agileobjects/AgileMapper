namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    // ReSharper disable once UnusedTypeParameter
    public interface IStringConverterTest<TTarget>
    {
        void ShouldProjectAParseableString();

        void ShouldProjectANullString();
    }
}