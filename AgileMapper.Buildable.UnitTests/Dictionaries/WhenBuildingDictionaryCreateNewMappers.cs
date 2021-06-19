namespace AgileObjects.AgileMapper.Buildable.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingDictionaryCreateNewMappers
    {
        [Fact]
        public void ShouldBuildANestedComplexTypeToStringDictionaryMapper()
        {
            var source = new PublicTwoFields<int, Address>
            {
                Value1 = 12345,
                Value2 = new Address { Line1 = "Line 1!", Line2 = "Line 2!" }
            };

            var result = GeneratedMapper.Map(source).ToANew<Dictionary<string, string>>();

            result.ShouldNotBeNull();
            result.ShouldContainKeyAndValue("Value1", "12345");
            result.ShouldContainKeyAndValue("Value2.Line1", "Line 1!");
            result.ShouldContainKeyAndValue("Value2.Line2", "Line 2!");
        }

        #region Configuration

        public class DictionaryCreateNewMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<PublicTwoFields<int, Address>>().ToANew<Dictionary<string, string>>();
            }
        }

        #endregion
    }
}
