namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<Ef6TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectAComplexTypeMemberToAFlatTypeList()
            => DoShouldProjectAComplexTypeMemberToAFlatTypeList();
    }
}
