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

            result.ShouldBeSameAs(target);
            result["Line1"].ShouldBe("Here");
            result["Line2"].ShouldBeNull();
            result["Line3"].ShouldBe("La la la");
        }
    }
}
