namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        [Fact]
        public void ShouldOverwriteAComplexTypeCollectionToADictionary()
        {
            var source = new Collection<Address>
            {
                new Address { Line1 = "1.1", Line2 = "1.2"},
                new Address { Line1 = "2.1", Line2 = null }
            };
            var target = new Dictionary<string, object>
            {
                ["[0].Line1"] = "Old 1.1",
                ["[0].Line2"] = "Old 1.2",
                ["[1].Line1"] = "Old 2.1",
                ["[1].Line2"] = "Old 2.2"
            };
            var result = Mapper.Map(source).Over(target);

            result["[0].Line1"].ShouldBe("1.1");
            result["[0].Line2"].ShouldBe("1.2");
            result["[1].Line1"].ShouldBe("2.1");
            result["[1].Line2"].ShouldBeNull();
        }
    }
}
