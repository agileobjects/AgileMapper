namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System.Threading.Tasks;

    public interface IStringConverterTest
    {
        Task ShouldProjectAParseableString();

        Task ShouldProjectANullString();
    }
}