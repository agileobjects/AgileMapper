namespace AgileObjects.AgileMapper.Buildable.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Mappers.Extensions;
    using Xunit;

    public class WhenBuildingSourceDictionaryEnumerableMappers
    {
        [Fact]
        public void ShouldBuildAStringDictionaryToAddressArrayMapper()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].Line1"] = "Line 1.1",
                ["[0].Line2"] = "Line 1.2",
                ["[1].Line1"] = "Line 2.1",
                ["[2].Line1"] = "Line 3.1",
                ["[2].Line2"] = "Line 3.2",
                ["[3].Line2"] = "Line 4.2",
            };

            var result = source.Map().ToANew<Address[]>();

            result.ShouldNotBeNull();
            result.Length.ShouldBe(4);
            result.First().Line1.ShouldBe("Line 1.1");
            result.First().Line2.ShouldBe("Line 1.2");
            
            result.Second().Line1.ShouldBe("Line 2.1");
            result.Second().Line2.ShouldBeNull();
            
            result.Third().Line1.ShouldBe("Line 3.1");
            result.Third().Line2.ShouldBe("Line 3.2");

            result.Fourth().Line1.ShouldBeNull();
            result.Fourth().Line2.ShouldBe("Line 4.2");
        }
    }
}