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
            var source = new TwoValues { Value = new int[5], value = string.Empty };
            var targetMember = TargetMemberFor<PublicProperty<byte>>(x => x.Value);

            var mappingContext = new MappingExecutor<TwoValues>(new MappingRuleSetCollection().CreateNew, MapperContext.Default);
            var rootCreationContext = mappingContext.CreateRootMappingContextData(source, targetMember);

            var childMapperData = new MemberMapperData(targetMember, rootCreationContext.MapperData);
            var childMappingContext = rootCreationContext.GetChildContextData(childMapperData);

            var matchingSourceMember = SourceMemberMatcher.GetMatchFor(childMappingContext);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("value");
        }

        private class TwoValues
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once NotAccessedField.Local
            public string value;

            public TwoValues()
            {
                value = string.Empty;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int[] Value { get; set; }
        }
    }
}