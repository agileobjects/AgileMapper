namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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

            result.ShouldContainKey("Value");
            result["Value"].ShouldBeOfType<Product>();
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberToAnUntypedDictionary()
        {
            var source = new PublicTwoFields<int, Address>
            {
                Value1 = 123,
                Value2 = new Address { Line1 = "One!" }
            };
            
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result.ShouldContainKeyAndValue("Value1", "123");
            result.ShouldContainKeyAndValue("Value2.Line1", "One!");
            result.ShouldContainKeyAndValue("Value2.Line2", null);
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
            result.ShouldNotContainKey("Address");
            result["Address.Line1"].ShouldBe("Customer house");
            result["Address.Line2"].ShouldBeNull();
        }

        [Fact]
        public void ShouldMapNestedSimpleTypeMembersToAnUntypedDictionary()
        {
            var source = new PublicProperty<PublicField<int>>
            {
                Value = new PublicField<int> { Value = 12345 }
            };
            var result = Mapper.Map(source).ToANew<Dictionary<string, object>>();

            result.ShouldNotContainKey("Value");
            result["Value.Value"].ShouldBe(12345);
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
            result.ShouldNotContainKey("[0]");

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

            result.ShouldNotContainKey("Value1");
            result.ShouldNotContainKey("Value2");

            result["Value1[0].Name"].ShouldBe("Clare");
            result.ShouldNotContainKey("Value1[0].Address");
            result["Value1[0].Address.Line1"].ShouldBe("Nes");
            result["Value1[0].Address.Line2"].ShouldBe("Ted");

            result["Value1[1].Name"].ShouldBe("Jim");
            result.ShouldNotContainKey("Value1[1].Address");
            result.ShouldNotContainKey("Value1[1].Address.Line1");
            result.ShouldNotContainKey("Value1[1].Address.Line2");

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

            result.Count.ShouldBe(2);
            result["1"].ShouldBe(1);
            result["2"].ShouldBe(2);
        }

        [Fact]
        public void ShouldMapBetweenDifferentSimpleValueTypeDictionaries()
        {
            var source = new Dictionary<string, char> { ["One"] = '1', ["Two"] = '2' };
            var result = Mapper.Map(source).ToANew<Dictionary<string, long>>();

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

            result.ShouldContainKey("[0]");
            result["[0]"].ShouldBe("Hello");
            result.ShouldContainKey("[1]");
            result["[1]"].ShouldBe("Goodbye");
            result.ShouldContainKey("[2]");
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

            result.ShouldContainKey("One");
            result["One"].ShouldBe("One!");

            result.ShouldContainKey("Two");
            result["Two"].ShouldBe("Two!");

            result.ShouldContainKey("Three");
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

            result.ShouldContainKey("Hello");
            result["Hello"].ShouldBe("Bonjour");

            result.ShouldContainKey("Yes");
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

            result.ShouldContainKey("One");
            result["One"].Line1.ShouldBe("1.1");
            result["One"].Line2.ShouldBe("1.2");

            result.ShouldContainKey("Two");
            result["Two"].Line1.ShouldBe("2.1");
            result["Two"].Line2.ShouldBe("2.2");

            result.ShouldContainKey("Three");
            result["Three"].ShouldBeNull();
        }

        [Fact]
        public void ShouldFlattenToValueTypes()
        {
            var source = new
            {
                Name = "Fred",
                Array = new[] { 1, 2, 3 },
                ComplexList = new List<PublicTwoFields<byte[], PublicField<int>>>
                {
                    new PublicTwoFields<byte[], PublicField<int>>
                    {
                        Value1 = new byte[] { 4, 8, 16 },
                        Value2 = new PublicField<int> { Value = 456 }
                    },
                    new PublicTwoFields<byte[], PublicField<int>>
                    {
                        Value1 = default(byte[]),
                        Value2 = new PublicField<int> { Value = 789 }
                    }
                }
            };

            var anonResult = Mapper.Map(source).ToANew<Dictionary<string, ValueType>>();

            // String members won't be mapped because they're not value types
            anonResult.ShouldNotContainKey("Name");
            anonResult["Array[0]"].ShouldBe(1);
            anonResult["Array[1]"].ShouldBe(2);
            anonResult["Array[2]"].ShouldBe(3);

            anonResult["ComplexList[0].Value1[0]"].ShouldBe(4);
            anonResult["ComplexList[0].Value1[1]"].ShouldBe(8);
            anonResult["ComplexList[0].Value1[2]"].ShouldBe(16);
            anonResult["ComplexList[0].Value2.Value"].ShouldBe(456);

            anonResult.ShouldNotContainKey("ComplexList[1].Value1");
            anonResult.ShouldNotContainKey("ComplexList[1].Value1[0]");
            anonResult["ComplexList[1].Value2.Value"].ShouldBe(789);
        }

        // See https://github.com/agileobjects/AgileMapper/issues/66
        [Fact]
        public void ShouldMapToAGivenDictionaryTypeObject()
        {
            var source = new PublicProperty<int> { Value = 6473 };
            var result = Mapper.Map(source).ToANew(typeof(Dictionary<string, string>));

            result.ShouldBeOfType<Dictionary<string, string>>();
            ((Dictionary<string, string>)result)["Value"].ShouldBe(6473);
        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            var source = new MysteryCustomer { Name = "Richie", Address = null };
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result["Name"].ShouldBe("Richie");
            result.ShouldNotContainKey("Address.Line1");
            result.ShouldNotContainKey("Address.Line2");
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

        [Fact]
        public void ShouldMapDictionaryKeys()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new StringKeyedDictionary<A> { ["Key1"] = new A(), ["Key2"] = new A() })
                    .ToANew<StringKeyedDictionary<B>>(cfg => cfg
                        .WhenMapping
                        .From<A>()
                        .ToANew<B>()
                        .Map(ctx => ctx.Parent.GetSource<StringKeyedDictionary<A>>().Keys.ElementAt(ctx.ElementIndex.Value))
                        .To(t => t.Something));

                result.Count.ShouldBe(2);

                result.ShouldContainKey("Key1");
                result["Key1"].ShouldNotBeNull();
                result["Key1"].Something.ShouldBe("Key1");

                result.ShouldContainKey("Key2");
                result["Key2"].ShouldNotBeNull();
                result["Key2"].Something.ShouldBe("Key2");
            }
        }

        public class A { }

        public class B
        {
            public string Something { get; set; }
        }
    }
}
