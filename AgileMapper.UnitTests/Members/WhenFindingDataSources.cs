namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenFindingDataSources : MemberTestsBase
    {
        [Fact]
        public void ShouldNotMatchSameNameIncompatibleTypeProperties()
        {
            var source = new TwoValues { Value = new int[5], value = string.Empty };
            var targetMember = TargetMemberFor<PublicProperty<byte>>(x => x.Value);

            var mappingContext = new MappingExecutor<TwoValues>(new MappingRuleSetCollection().CreateNew, MapperContext.Default);
            var rootMappingData = mappingContext.CreateRootMappingData(source, targetMember);
            var rootMapperData = rootMappingData.MapperData;

            var childMapperData = new ChildMemberMapperData(targetMember, rootMapperData);
            var childMappingContext = rootMappingData.GetChildMappingData(childMapperData);

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