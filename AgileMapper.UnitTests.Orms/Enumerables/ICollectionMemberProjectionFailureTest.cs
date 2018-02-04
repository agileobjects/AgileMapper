namespace AgileObjects.AgileMapper.UnitTests.Orms.Enumerables
{
    using System.Threading.Tasks;

    public interface ICollectionMemberProjectionFailureTest
    {
        Task ShouldErrorProjectingToAComplexTypeCollectionMember();
    }
}