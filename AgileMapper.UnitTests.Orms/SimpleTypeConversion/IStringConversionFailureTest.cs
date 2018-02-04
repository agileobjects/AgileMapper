namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Threading.Tasks;

    public interface IStringConversionFailureTest
    {
        Task ShouldErrorProjectingAParseableString();

        Task ShouldErrorProjectingANullString();
    }
}