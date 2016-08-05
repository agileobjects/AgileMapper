namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using DataSources;
    using ObjectPopulation;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenFindingDataSources : MemberFinderTestsBase
    {
        private static readonly DataSourceFinder _dataSourceFinder = new DataSourceFinder(GlobalContext.Instance);

        [Fact]
        public void ShouldNotMatchSameNameIncompatibleTypeProperties()
        {
            var source = new { Value = new int[5], value = string.Empty };
            var targetMember = TargetMemberFor<PublicProperty<byte>>(x => x.Value);

            var mappingContext = new MappingContext(new MappingRuleSetCollection().CreateNew, MapperContext.Default);
            var rootObjectMappingContext = ObjectMappingContextFactory.CreateRoot(source, default(PublicProperty<byte>), mappingContext);
            var memberMappingContext = new MemberMappingContext(targetMember, rootObjectMappingContext);

            var matchingSourceMember = _dataSourceFinder.GetSourceMemberFor(memberMappingContext);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("value");
        }
    }
}