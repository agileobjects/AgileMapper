namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Threading.Tasks;

    public interface IStringConversionValidationFailureTest
    {
        Task ShouldErrorProjectingAnUnparseableString();
    }
}