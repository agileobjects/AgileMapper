namespace AgileObjects.AgileMapper.UnitTests.Orms.Recursion
{
    using System.Threading.Tasks;

    public interface IOneToManyRecursionProjectorTest
    {
        Task ShouldProjectAOneToManyRelationshipToDefaultRecursionDepth();

        Task ShouldProjectAOneToManyRelationshipToFirstRecursionDepth();

        Task ShouldProjectAOneToManyRelationshipToSecondRecursionDepth();
    }
}