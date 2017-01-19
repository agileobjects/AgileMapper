namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverDictionaries
    {
        [Fact]
        public void ShouldOverwriteASimpleTypedDictionary()
        {
            var source = new Address { Line1 = "Here", Line2 = "There" };
            var target = new Dictionary<string, string> { ["Line1"] = "La la la" };
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result["Line1"].ShouldBe("Here");
            result["Line2"].ShouldBe("There");
        }

        [Fact]
        public void ShouldNotRemoveUnmatchedEntries()
        {
            var source = new Address { Line1 = "Here", Line2 = null };
            var target = new Dictionary<string, object> { ["Line3"] = "La la la" };
            var result = Mapper.Map(source).Over(target);

            result["Line1"].ShouldBe("Here");
            result["Line2"].ShouldBeNull();
            result["Line3"].ShouldBe("La la la");
        }

        [Fact]
        public void ShouldOverwriteASimpleTypeListToADictionary()
        {
            var source = new List<int> { 1, 2, 3 };
            var target = new Dictionary<string, string>
            {
                ["[0]"] = "9",
                ["[1]"] = "8",
                ["[4]"] = "6"
            };
            var result = Mapper.Map(source).Over(target);

            result["[0]"].ShouldBe("1");
            result["[1]"].ShouldBe("2");
            result["[2]"].ShouldBe("3");
            result["[4]"].ShouldBe("6");
        }
    }
}
