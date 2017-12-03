﻿namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewDictionaries
    {
        [Fact]
        public void ShouldMapASimpleTypeMemberToAnUntypedDictionary()
        {
            var source = new PublicField<long> { Value = long.MinValue };
            var result = Mapper.Map(source).ToANew<Dictionary<string, object>>();

            result.ShouldNotBeEmpty();
            result["Value"].ShouldBe(long.MinValue);
        }

        [Fact]
        public void ShouldMapASimpleTypeMemberToATypedDictionary()
        {
            var source = new PublicProperty<int> { Value = int.MaxValue };
            var result = Mapper.Map(source).ToANew<Dictionary<string, int>>();

            result["Value"].ShouldBe(int.MaxValue);
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberToATypedDictionary()
        {
            var source = new PublicProperty<Product> { Value = new Product { ProductId = "xxx" } };
            var result = Mapper.Map(source).ToANew<Dictionary<string, Product>>();

            result.ContainsKey("Value").ShouldBeTrue();
            result["Value"].ShouldBeOfType<Product>();
        }

        [Fact]
        public void ShouldMapASimpleTypeMemberToAConvertibleTypedDictionary()
        {
            var source = new PublicGetMethod<string>("6473");
            var result = Mapper.Map(source).ToANew<Dictionary<string, short>>();

            result["GetValue"].ShouldBe(6473);
        }

        [Fact]
        public void ShouldMapNestedSimpleTypeMembersToATypedDictionary()
        {
            var source = new MysteryCustomer
            {
                Name = "Eddie",
                Address = new Address { Line1 = "Customer house" }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result["Name"].ShouldBe("Eddie");
            result["Address.Line1"].ShouldBe("Customer house");
            result["Address.Line2"].ShouldBeNull();
        }

        [Fact]
        public void ShouldMapASimpleTypeArrayToAnUntypedDictionary()
        {
            var source = new[] { 5, 6, 7, 8 };
            var result = Mapper.Map(source).ToANew<Dictionary<string, object>>();

            result.Count.ShouldBe(4);
            result["[0]"].ShouldBe(5);
            result["[1]"].ShouldBe(6);
            result["[2]"].ShouldBe(7);
            result["[3]"].ShouldBe(8);
        }

        [Fact]
        public void ShouldMapASimpleTypeListToAConvertibleTypedDictionary()
        {
            var source = new List<string> { "8", "7", "6" };
            var result = Mapper.Map(source).ToANew<Dictionary<object, short>>();

            result.Count.ShouldBe(3);
            result["[0]"].ShouldBe(8);
            result["[1]"].ShouldBe(7);
            result["[2]"].ShouldBe(6);
        }

        [Fact]
        public void ShouldMapAComplexTypeCollectionToAnUntypedDictionary()
        {
            var source = new Collection<Address>
            {
                new Address { Line1 = "LOL" },
                new Address { Line1 = "ROFL", Line2 = "YOLO" }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, object>>();

            result.Count.ShouldBe(4);
            result.ContainsKey("[0]").ShouldBeFalse();

            result["[0].Line1"].ShouldBe("LOL");
            result["[0].Line2"].ShouldBeNull();

            result["[1].Line1"].ShouldBe("ROFL");
            result["[1].Line2"].ShouldBe("YOLO");
        }

        [Fact]
        public void ShouldMapNestedComplexAndSimpleTypeEnumerablesToAnUntypedDictionary()
        {
            var now = DateTime.Now;
            var source = new PublicTwoFields<IEnumerable<Person>, ICollection<DateTime>>
            {
                Value1 = new[]
                {
                    new Person { Name = "Clare", Address = new Address { Line1 = "Nes", Line2 = "Ted" } },
                    new Person { Name = "Jim" }
                },
                Value2 = new[] { now.AddMinutes(1), now.AddMinutes(2), now.AddMinutes(3) }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, object>>();

            result.ContainsKey("Value1").ShouldBeFalse();
            result.ContainsKey("Value2").ShouldBeFalse();

            result["Value1[0].Name"].ShouldBe("Clare");
            result.ContainsKey("Value1[0].Address").ShouldBeFalse();
            result["Value1[0].Address.Line1"].ShouldBe("Nes");
            result["Value1[0].Address.Line2"].ShouldBe("Ted");

            result["Value1[1].Name"].ShouldBe("Jim");
            result.ContainsKey("Value1[1].Address").ShouldBeFalse();
            result.ContainsKey("Value1[1].Address.Line1").ShouldBeFalse();
            result.ContainsKey("Value1[1].Address.Line2").ShouldBeFalse();

            result["Value2[0]"].ShouldBe(now.AddMinutes(1));
            result["Value2[1]"].ShouldBe(now.AddMinutes(2));
            result["Value2[2]"].ShouldBe(now.AddMinutes(3));
        }

        [Fact]
        public void ShouldMapBetweenSameSimpleValueTypedDictionaries()
        {
            var source = new Dictionary<string, int> { ["One"] = 1, ["Two"] = 2 };
            var result = Mapper.Map(source).ToANew<IDictionary<string, int>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);
            result["One"].ShouldBe(1);
            result["Two"].ShouldBe(2);
        }

        [Fact]
        public void ShouldMapBetweenDifferentSimpleKeyTypeIDictionaries()
        {
            IDictionary<int, int> source = new Dictionary<int, int> { [1] = 1, [2] = 2 };
            var result = Mapper.Map(source).ToANew<IDictionary<string, int>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);
            result["1"].ShouldBe(1);
            result["2"].ShouldBe(2);
        }

        [Fact]
        public void ShouldMapBetweenDifferentSimpleValueTypeDictionaries()
        {
            var source = new Dictionary<string, char> { ["One"] = '1', ["Two"] = '2' };
            var result = Mapper.Map(source).ToANew<Dictionary<string, long>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);
            result["One"].ShouldBe(1);
            result["Two"].ShouldBe(2);
        }

        [Fact]
        public void ShouldMapBetweenSameComplexValueTypedDictionaries()
        {
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();

            var source = new Dictionary<Guid, Address>
            {
                [key1] = new Address { Line1 = "Address 1" },
                [key2] = new Address { Line1 = "Address 2 Line 1", Line2 = "Address 2 Line 2" }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<Guid, Address>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);

            result[key1].ShouldNotBeSameAs(source[key1]);
            result[key1].Line1.ShouldBe("Address 1");

            result[key2].ShouldNotBeSameAs(source[key2]);
            result[key2].Line1.ShouldBe("Address 2 Line 1");
            result[key2].Line2.ShouldBe("Address 2 Line 2");
        }

        [Fact]
        public void ShouldMapBetweenDifferentComplexValueTypedDictionaries()
        {
            var source = new Dictionary<int, PersonViewModel>
            {
                [123] = new PersonViewModel { Name = "Bobby", AddressLine1 = "Address 1" },
                [456] = new PersonViewModel { Name = "Magnus", AddressLine1 = "Address 2!" }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, Person>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);

            result["123"].Name.ShouldBe("Bobby");
            result["123"].Address.Line1.ShouldBe("Address 1");

            result["456"].Name.ShouldBe("Magnus");
            result["456"].Address.Line1.ShouldBe("Address 2!");
        }

        [Fact]
        public void ShouldMapTypePairsBetweenDifferentComplexValueTypedDictionaries()
        {
            var source = new Dictionary<int, PersonViewModel>
            {
                [123] = new PersonViewModel { Name = "Bobby", AddressLine1 = "Address 1" },
                [456] = new CustomerViewModel { Name = "Magnus", AddressLine1 = "Address 2!", Discount = 0.25 }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, Person>>();

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);

            result["123"].Name.ShouldBe("Bobby");
            result["123"].Address.Line1.ShouldBe("Address 1");

            result["456"].ShouldBeOfType<Customer>();
            result["456"].Name.ShouldBe("Magnus");
            result["456"].Address.Line1.ShouldBe("Address 2!");
            ((Customer)result["456"]).Discount.ShouldBe(0.25m);
        }

        [Fact]
        public void ShouldMapToASimpleTypeDictionaryImplementation()
        {
            var source = new[] { "Hello", "Goodbye", "See ya" };
            var result = Mapper.Map(source).ToANew<StringKeyedDictionary<string>>();

            result.Count.ShouldBe(3);

            result.ContainsKey("[0]").ShouldBeTrue();
            result["[0]"].ShouldBe("Hello");
            result.ContainsKey("[1]").ShouldBeTrue();
            result["[1]"].ShouldBe("Goodbye");
            result.ContainsKey("[2]").ShouldBeTrue();
            result["[2]"].ShouldBe("See ya");
        }

        [Fact]
        public void ShouldMapFromASimpleTypeDictionaryImplementationToAnIDictionary()
        {
            var source = new StringKeyedDictionary<string>
            {
                ["One"] = "One!",
                ["Two"] = "Two!",
                ["Three"] = "Three!",
            };
            var result = Mapper.Map(source).ToANew<IDictionary<string, string>>();

            result.Count.ShouldBe(3);

            result.ContainsKey("One").ShouldBeTrue();
            result["One"].ShouldBe("One!");

            result.ContainsKey("Two").ShouldBeTrue();
            result["Two"].ShouldBe("Two!");

            result.ContainsKey("Three").ShouldBeTrue();
            result["Three"].ShouldBe("Three!");
        }

        [Fact]
        public void ShouldMapBetweenSameDeclaredSimpleTypeIDictionaries()
        {
            IDictionary<string, string> source = new StringKeyedDictionary<string>
            {
                ["Hello"] = "Bonjour",
                ["Yes"] = "Oui"
            };
            var result = Mapper.Map(source).ToANew<IDictionary<string, string>>();

            result.Count.ShouldBe(2);

            result.ContainsKey("Hello").ShouldBeTrue();
            result["Hello"].ShouldBe("Bonjour");

            result.ContainsKey("Yes").ShouldBeTrue();
            result["Yes"].ShouldBe("Oui");
        }

        [Fact]
        public void ShouldMapBetweenSameComplexTypeDictionaryImplementations()
        {
            var source = new StringKeyedDictionary<Address>
            {
                ["One"] = new Address { Line1 = "1.1", Line2 = "1.2" },
                ["Two"] = new Address { Line1 = "2.1", Line2 = "2.2" },
                ["Three"] = default(Address),
            };
            var result = Mapper.Map(source).ToANew<StringKeyedDictionary<Address>>();

            result.Count.ShouldBe(3);

            result.ContainsKey("One").ShouldBeTrue();
            result["One"].Line1.ShouldBe("1.1");
            result["One"].Line2.ShouldBe("1.2");

            result.ContainsKey("Two").ShouldBeTrue();
            result["Two"].Line1.ShouldBe("2.1");
            result["Two"].Line2.ShouldBe("2.2");

            result.ContainsKey("Three").ShouldBeTrue();
            result["Three"].ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            var source = new MysteryCustomer { Name = "Richie", Address = null };
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result["Name"].ShouldBe("Richie");
            result.ContainsKey("Address.Line1").ShouldBeFalse();
            result.ContainsKey("Address.Line2").ShouldBeFalse();
        }

        [Fact]
        public void ShouldHandleANullEnumerableMember()
        {
            var source = new PublicGetMethod<int[]>(value: null);
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldHandleAnInvalidKeyTypeTargetDictionary()
        {
            var source = new PublicField<string> { Value = "DateTime keys?!" };
            var result = Mapper.Map(source).ToANew<Dictionary<DateTime, string>>();

            result.ShouldBeEmpty();
        }
    }
}
