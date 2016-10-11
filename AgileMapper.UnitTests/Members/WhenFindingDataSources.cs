namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
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
            var rootCreationContext = mappingContext.CreateRootMappingContextData(source, targetMember);

            var childMapperData = new MemberMapperData(targetMember, rootCreationContext.MapperData);
            var childMappingContext = rootCreationContext.GetChildContextData(childMapperData);

            var matchingSourceMember = SourceMemberMatcher.GetMatchFor(childMappingContext);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("value");
        }
    }
}