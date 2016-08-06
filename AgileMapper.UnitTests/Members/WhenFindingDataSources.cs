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
        [Fact]
        public void ShouldNotMatchSameNameIncompatibleTypeProperties()
        {
            var source = new { Value = new int[5], value = string.Empty };
            var targetMember = TargetMemberFor<PublicProperty<byte>>(x => x.Value);

            var mappingContext = new MappingContext(new MappingRuleSetCollection().CreateNew, MapperContext.Default);
            var rootObjectMappingContext = ObjectMappingContextFactory.CreateRoot(source, default(PublicProperty<byte>), mappingContext);
            var memberMappingContext = new MemberMappingContext(targetMember, rootObjectMappingContext);

            var matchingSourceMember = SourceMemberMatcher.GetMatchFor(memberMappingContext);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("value");
        }
    }
}