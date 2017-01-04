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

        [Fact]
        public void ShouldMapANestedSimpleTypeArrayToANestedTypedDictionary()
        {
            var source = new PublicProperty<string[]> { Value = new[] { "12", "13.6", "hjf", "99.009" } };
            var result = Mapper.Map(source).ToANew<PublicField<Dictionary<object, decimal?>>>();

            result.Value.Count.ShouldBe(4);
            result.Value["[0]"].ShouldBe(12);
            result.Value["[1]"].ShouldBe(13.6);
            result.Value["[2]"].ShouldBeNull();
            result.Value["[3]"].ShouldBe(99.009);
        }

        [Fact]
        public void ShouldMapANestedComplexTypeArrayToANestedTypedDictionary()
        {
            var source = new PublicProperty<ICollection<CustomerViewModel>>
            {
                Value = new List<CustomerViewModel>
                {
                    new CustomerViewModel { Name = "Cat", Discount = 0.5 },
                    new CustomerViewModel { Name = "Dog", Discount = 0.6 }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Dictionary<string, decimal>>>();

            result.Value.ContainsKey("[0]").ShouldBeFalse();

            result.Value["[0].Name"].ShouldBeDefault();
            result.Value.ContainsKey("[0].Id").ShouldBeFalse(); // <- because id is a Guid, which can't be parsed to a decimal
            result.Value["[0].AddressLine1"].ShouldBeDefault();
            result.Value["[0].Discount"].ShouldBe(0.5);

            result.Value["[1].Name"].ShouldBeDefault();
            result.Value.ContainsKey("[1].Id").ShouldBeFalse();
            result.Value["[1].AddressLine1"].ShouldBeDefault();
            result.Value["[1].Discount"].ShouldBe(0.6);
        }

        [Fact]
        public void ShouldFlattenANestedEnumerableOfEnumerablesToANestedTypedDictionary()
        {
            var source = new PublicProperty<int[][]>
            {
                Value = new[]
                {
                    new [] { 1,2,3 },
                    new [] { 4,5,6 }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Dictionary<string, double>>>();

            result.Value.ContainsKey("[0]").ShouldBeFalse();

            result.Value["[0][0]"].ShouldBe(1);
            result.Value["[0][1]"].ShouldBe(2);
            result.Value["[0][2]"].ShouldBe(3);
            result.Value["[1][0]"].ShouldBe(4);
            result.Value["[1][1]"].ShouldBe(5);
            result.Value["[1][2]"].ShouldBe(6);
        }
    }
}
