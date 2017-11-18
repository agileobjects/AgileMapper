namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    // ReSharper disable once UnusedTypeParameter
    public interface IStringConversionValidationFailureTest<TTarget>
    {
        void ShouldErrorProjectingAnUnparseableString();
    }
}