namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Configuration
{
    using System.Threading.Tasks;
    using Infrastructure;
    using Orms.Configuration;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<Ef6TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEf6TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstant() => DoShouldApplyAConfiguredConstant();

        [Fact]
        public Task ShouldConditionallyApplyAConfiguredConstant()
            => DoShouldConditionallyApplyAConfiguredConstant();

        [Fact]
        public Task ShouldApplyAConfiguredConstantToANestedMember()
            => DoShouldApplyAConfiguredConstantToANestedMember();

        [Fact]
        public Task ShouldApplyAConfiguredMember() => DoShouldApplyAConfiguredMember();

        [Fact]
        public Task ShouldApplyMultipleConfiguredMembers() => DoShouldApplyMultipleConfiguredMembers();

        [Fact]
        public Task ShouldConditionallyApplyAConfiguredMember() => DoShouldConditionallyApplyAConfiguredMember();

        [Fact]
        public Task ShouldApplyConditionalAndUnconditionalDataSourcesInOrder()
            => DoShouldApplyConditionalAndUnconditionalDataSourcesInOrder();

        [Fact]
        public Task ShouldHandleANullMemberInACondition() => DoShouldHandleANullMemberInACondition();

        [Fact]
        public Task ShouldSupportMultipleDivergedMappers() => DoShouldSupportMultipleDivergedMappers();
    }
}