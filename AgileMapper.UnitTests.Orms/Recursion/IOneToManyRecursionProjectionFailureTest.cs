namespace AgileObjects.AgileMapper.UnitTests.Orms.Recursion
{
    using System.Threading.Tasks;

    public interface IOneToManyRecursionProjectionFailureTest
    {
        Task ShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth();
    }
}