namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingFromDictionariesToNewComplexTypeMembers
    {
        [Fact]
        public void ShouldMapToAStringMemberFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string> { ["Value.Value"] = "Over there!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<string>>>();

            result.Value.Value.ShouldBe("Over there!");
        }

        [Fact]
        public void ShouldMapToABoolMemberFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object> { ["Value.Value"] = "true" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicProperty<bool>>>();

            result.Value.Value.ShouldBeTrue();
        }

        [Fact]
        public void ShouldMapToMemberFromFlattenedNameEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["Name"] = "Bob",
                ["Discount"] = "0.1",
                ["AddressLine1"] = "Bob's House",
                ["AddressLine2"] = "Bob's Street"
            };
            var result = Mapper.Map(source).ToANew<Customer>();

            result.Name.ShouldBe("Bob");
            result.Discount.ShouldBe(0.1);
            result.Address.Line1.ShouldBe("Bob's House");
            result.Address.Line2.ShouldBe("Bob's Street");
        }

        [Fact]
        public void ShouldMapToNestedMembersFromUntypedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Id"] = 123,
                ["Name"] = "Captain Customer",
                ["Address"] = new Address
                {
                    Line1 = "Line 1",
                    Line2 = "Line 2"
                }
            };
            var result = Mapper.Map(source).ToANew<Customer>();

            result.Id.ShouldBeDefault();
            result.Name.ShouldBe("Captain Customer");
            result.Address.ShouldNotBeNull();
            result.Address.Line1.ShouldBe("Line 1");
            result.Address.Line2.ShouldBe("Line 2");
        }

        [Fact]
        public void ShouldMapToDeepNestedComplexTypeMembersFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0].Value.SetValue[0].Title"] = "Mr",
                ["Value[0].Value.SetValue[0].Name"] = "Franks",
                ["Value[0].Value.SetValue[0].Address.Line1"] = "Somewhere",
                ["Value[0].Value.SetValue[0].Address.Line2"] = "Over the rainbow",
                ["Value[0].Value.SetValue[1]"] = new PersonViewModel { Name = "Mike", AddressLine1 = "La la la" },
                ["Value[0].Value.SetValue[2].Title"] = 5,
                ["Value[0].Value.SetValue[2].Name"] = "Wilkes",
                ["Value[0].Value.SetValue[2].Address.Line1"] = "Over there",
                ["Value[1].Value.SetValue[0].Title"] = 737328,
                ["Value[1].Value.SetValue[0].Name"] = "Rob",
                ["Value[1].Value.SetValue[0].Address.Line1"] = "Some place"
            };

            var result = Mapper
                .Map(source)
                .ToANew<PublicField<ICollection<PublicProperty<PublicSetMethod<Person[]>>>>>();

            result.Value.Count.ShouldBe(2);

            result.Value.First().Value.Value.Length.ShouldBe(3);
            result.Value.Second().Value.Value.Length.ShouldBe(1);

            result.Value.First().Value.Value.First().Title.ShouldBe(Title.Mr);
            result.Value.First().Value.Value.First().Name.ShouldBe("Franks");
            result.Value.First().Value.Value.First().Address.Line1.ShouldBe("Somewhere");
            result.Value.First().Value.Value.First().Address.Line2.ShouldBe("Over the rainbow");

            result.Value.First().Value.Value.Second().Title.ShouldBeDefault();
            result.Value.First().Value.Value.Second().Name.ShouldBe("Mike");
            result.Value.First().Value.Value.Second().Address.Line1.ShouldBe("La la la");
            result.Value.First().Value.Value.Second().Address.Line2.ShouldBeDefault();

            result.Value.First().Value.Value.Third().Title.ShouldBe(Title.Mrs);
            result.Value.First().Value.Value.Third().Name.ShouldBe("Wilkes");
            result.Value.First().Value.Value.Third().Address.Line1.ShouldBe("Over there");
            result.Value.First().Value.Value.Third().Address.Line2.ShouldBeDefault();

            result.Value.Second().Value.Value.First().Title.ShouldBeDefault();
            result.Value.Second().Value.Value.First().Name.ShouldBe("Rob");
            result.Value.Second().Value.Value.First().Address.Line1.ShouldBe("Some place");
            result.Value.Second().Value.Value.First().Address.Line2.ShouldBeDefault();
        }
    }
}