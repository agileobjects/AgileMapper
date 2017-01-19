namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverDictionaryMembers
    {
        [Fact]
        public void ShouldOverwriteANestedSimpleTypedIDictionary()
        {
            var source = new PublicField<Address>
            {
                Value = new Address { Line1 = "Here", Line2 = "There" }
            };
            var target = new PublicProperty<IDictionary<string, string>>
            {
                Value = new Dictionary<string, string> { ["Line1"] = "La la la" }
            };
            var result = Mapper.Map(source).Over(target);

            result.Value["Line1"].ShouldBe("Here");
            result.Value["Line2"].ShouldBe("There");
        }
    }
}