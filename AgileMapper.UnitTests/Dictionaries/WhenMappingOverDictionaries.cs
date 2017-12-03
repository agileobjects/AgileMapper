namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.Extensions;
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
        public void ShouldOverwriteANestedSimpleConvertibleTypeEnumerableToAnIDictionary()
        {
            var source = new PublicProperty<IEnumerable<string>>
            {
                Value = new List<string> { "6", "7", "8" }
            };
            IDictionary<string, int> target = new Dictionary<string, int>
            {
                ["Value[0]"] = 9,
                ["Value[1]"] = 8,
                ["Value[4]"] = 5
            };
            var result = Mapper.Map(source).Over(target);

            result["Value[0]"].ShouldBe(6);
            result["Value[1]"].ShouldBe(7);
            result["Value[2]"].ShouldBe(8);
            result["Value[4]"].ShouldBe(5);
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

        [Fact]
        public void ShouldOverwriteAComplexTypeCollectionToASameComplexTypeDictionary()
        {
            var source = new Collection<Address>
            {
                new Address { Line1 = "1.1", Line2 = null},
                new Address { Line1 = "2.1", Line2 = "2.2" }
            };
            var target = new Dictionary<string, Address>
            {
                ["[0]"] = new Address { Line1 = "Old 1.1", Line2 = null },
                ["[1]"] = default(Address)
            };
            var existingAddress = target["[0]"];
            var result = Mapper.Map(source).Over(target);

            result["[0]"].ShouldBeSameAs(existingAddress);
            result["[0]"].Line1.ShouldBe("1.1");
            result["[0]"].Line2.ShouldBeNull();
            result["[1]"].ShouldNotBeNull();
            result["[1]"].Line1.ShouldBe("2.1");
            result["[1]"].Line2.ShouldBe("2.2");
        }

        [Fact]
        public void ShouldOverwriteAComplexTypeListToADifferentComplexTypeIDictionary()
        {
            var source = new List<MysteryCustomer>
            {
                new MysteryCustomer { Name = "Lois", Address = new Address { Line1 = null } },
                new MysteryCustomer { Name = "Clark", Address = new Address { Line1 = "Daily Planet" } }
            };
            IDictionary<string, MysteryCustomerViewModel> target = new Dictionary<string, MysteryCustomerViewModel>
            {
                ["[0]"] = new MysteryCustomerViewModel { Name = null, AddressLine1 = "Bugle" },
                ["[1]"] = new MysteryCustomerViewModel { Name = "Perry" },
            };
            var result = Mapper.Map(source).Over(target);

            result["[0]"].Name.ShouldBe("Lois");
            result["[0]"].AddressLine1.ShouldBeNull();
            result["[1]"].Name.ShouldBe("Clark");
            result["[1]"].AddressLine1.ShouldBe("Daily Planet");
        }

        [Fact]
        public void ShouldOverwriteASimpleTypeArrayToADictionaryImplementation()
        {
            var source = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var target = new StringKeyedDictionary<string>
            {
                ["[0]"] = source.First().ToString(),
                ["[1]"] = source.Third().ToString(),
                ["[4]"] = Guid.NewGuid().ToString()
            };
            var preMapping4 = target["[4]"];
            var result = Mapper.Map(source).Over(target);

            result.Count.ShouldBe(4);
            result["[0]"].ShouldBe(source.First().ToString());
            result["[1]"].ShouldBe(source.Second().ToString());
            result["[2]"].ShouldBe(source.Third().ToString());
            result["[4]"].ShouldBe(preMapping4);
        }
    }
}
