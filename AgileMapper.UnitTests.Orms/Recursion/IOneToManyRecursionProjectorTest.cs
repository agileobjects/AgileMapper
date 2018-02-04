namespace AgileObjects.AgileMapper.UnitTests.Orms.Recursion
{
    using System.Threading.Tasks;

    public interface IOneToManyRecursionProjectorTest
    {
        Task ShouldProjectAOneToManyRelationshipToZeroethRecursionDepth();

        Task ShouldProjectAOneToManyRelationshipToFirstRecursionDepth();

        Task ShouldProjectAOneToManyRelationshipToSecondRecursionDepth();

        Task ShouldProjectAOneToManyRelationshipToDefaultRecursionDepth();
    }
}