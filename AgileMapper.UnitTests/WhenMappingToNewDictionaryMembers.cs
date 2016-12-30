namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewDictionaryMembers
    {
        [Fact]
        public void ShouldMapNestedSimpleTypeMembersToANestedUntypedDictionary()
        {
            var source = new PublicProperty<Person>
            {
                Value = new Person
                {
                    Name = "Someone",
                    Address = new Address { Line1 = "Some Place" }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Dictionary<string, object>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeEmpty();
            result.Value["Name"].ShouldBe("Someone");
            result.Value.ContainsKey("Address").ShouldBeFalse();
            result.Value["Address.Line1"].ShouldBe("Some Place");
        }
    }
}
