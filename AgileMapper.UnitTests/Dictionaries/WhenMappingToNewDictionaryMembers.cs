namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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
            result.Value.ShouldNotContainKey("Address");
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
                    new MysteryCustomerViewModel { Name = "Dog", Discount = 0.6, Report = "0.075" }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Dictionary<string, decimal>>>();

            result.Value.ShouldNotContainKey("[0]");

            result.Value["[0].Name"].ShouldBeDefault();
            result.Value.ShouldNotContainKey("[0].Id"); // <- because id is a Guid, which can't be parsed to a decimal
            result.Value["[0].AddressLine1"].ShouldBeDefault();
            result.Value["[0].Discount"].ShouldBe(0.5);

            result.Value["[1].Name"].ShouldBeDefault();
            result.Value.ShouldNotContainKey("[1].Id");
            result.Value["[1].AddressLine1"].ShouldBeDefault();
            result.Value["[1].Discount"].ShouldBe(0.6);
            result.Value["[1].Report"].ShouldBe(0.075);
        }

        [Fact]
        public void ShouldFlattenANestedArrayOfArraysToANestedTypedDictionary()
        {
            var source = new PublicProperty<int[][]>
            {
                Value = new[]
                {
                    new [] { 1, 2, 3 },
                    new [] { 4, 5, 6 }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<IDictionary<string, double>>>();

            result.Value.ShouldNotContainKey("[0]");

            result.Value["[0][0]"].ShouldBe(1.0);
            result.Value["[0][1]"].ShouldBe(2.0);
            result.Value["[0][2]"].ShouldBe(3.0);
            result.Value["[1][0]"].ShouldBe(4.0);
            result.Value["[1][1]"].ShouldBe(5.0);
            result.Value["[1][2]"].ShouldBe(6.0);
        }

        [Fact]
        public void ShouldMapANestedEnumerableOfArraysToANestedEnumerableTypedDictionary()
        {
            var source = new PublicProperty<IEnumerable<int[]>>
            {
                Value = new[]
                {
                    new [] { 1, 2, 3 },
                    null,
                    new [] { 4, 5, 6 }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Dictionary<string, IEnumerable<string>>>>();

            result.Value.ShouldNotContainKey("[0][0]");

            result.Value["[0]"].ShouldBe("1", "2", "3");
            result.Value["[1]"].ShouldBeDefault();
            result.Value["[2]"].ShouldBe("4", "5", "6");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/8
        [Fact]
        public void ShouldMapFromARuntimeTypedNestedDictionary()
        {
            var source = new Dictionary<string, object>
            {
                ["Value1"] = "I'm Value1!",
                ["Value2"] = new Dictionary<string, object>
                {
                    ["Value"] = "I'm nested!"
                }
            };

            var result = Mapper.Map(source).ToANew<PublicTwoFields<string, PublicProperty<string>>>();

            result.Value1.ShouldBe("I'm Value1!");
            result.Value2.ShouldNotBeNull();
            result.Value2.Value.ShouldBe("I'm nested!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/10
        [Fact]
        public void ShouldMapADictionaryMemberToANewDictionaryMember()
        {
            var source = new PublicField<Dictionary<string, object>>()
            {
                Value = new Dictionary<string, object>
                {
                    ["Location"] = "I'm in a Dictionary!",
                    ["Number"] = 123,
                    ["Object"] = new CustomerViewModel { Name = "Mr Yo Yo" }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<Dictionary<string, object>>>();

            result.Value.ShouldNotBeNull();
            result.Value["Location"].ShouldBe("I'm in a Dictionary!");
            result.Value["Number"].ShouldBe(123);
            result.Value["Object"].ShouldBeOfType<CustomerViewModel>();
            result.Value["Object"].ShouldNotBeSameAs(source.Value["Object"]);
            ((CustomerViewModel)result.Value["Object"]).Name.ShouldBe("Mr Yo Yo");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/110
        [Fact]
        public void ShouldMapSimpleTypeObjectValuesToSimpleTypeObjectValues()
        {
            var source = new PublicField<Dictionary<string, object>>
            {
                Value = new Dictionary<string, object>
                {
                    { "int", 1 },
                    { "double", 1.0 },
                    { "decimal", 1m },
                    { "string", "hello" },
                    { "bool", true }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<Dictionary<string, object>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.Count.ShouldBe(source.Value.Count);
            result.Value.ShouldContainKeyAndValue("int", 1);
            result.Value.ShouldContainKeyAndValue("double", 1.0);
            result.Value.ShouldContainKeyAndValue("decimal", 1m);
            result.Value.ShouldContainKeyAndValue("string", "hello");
            result.Value.ShouldContainKeyAndValue("bool", true);
        }

        // See https://github.com/agileobjects/AgileMapper/issues/10
        [Fact]
        public void ShouldMapADictionaryObjectValuesToNewDictionaryObjectValues()
        {
            var source = new PublicField<Dictionary<string, object>>()
            {
                Value = new Dictionary<string, object>
                {
                    ["key1"] = new object(),
                    ["key2"] = new object()
                }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<Dictionary<string, object>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldContainKey("key1");
            result.Value["key1"].ShouldBeOfType<object>();
            result.Value["key1"].ShouldNotBeSameAs(source.Value["key1"]);
            result.Value.ShouldContainKey("key2");
            result.Value["key2"].ShouldBeOfType<object>();
            result.Value["key2"].ShouldNotBeSameAs(source.Value["key2"]);
        }

        // See https://github.com/agileobjects/AgileMapper/issues/97
        [Fact]
        public void ShouldDeepCloneAReadOnlyDictionaryMember()
        {
            var source = new Issue97.ReadonlyDictionary();

            source.Dictionary["Test"] = "123";

            var cloned = Mapper.DeepClone(source);

            cloned.Dictionary.ContainsKey("Test").ShouldBeTrue();
            cloned.Dictionary["Test"].ShouldBe("123");
        }

        [Fact]
        public void ShouldUseACloneConstructorToPopulateADictionaryConstructorParameter()
        {
            var source = new PublicReadOnlyProperty<IDictionary<string, string>>(
                new Dictionary<string, string> { ["Test"] = "Hello!" });

            var result = Mapper.Map(source).ToANew<PublicCtor<IDictionary<string, string>>>();

            result.Value.ContainsKey("Test").ShouldBeTrue();
            result.Value["Test"].ShouldBe("Hello!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/110
        [Fact]
        public void ShouldCloneSimpleTypeValuesInAnObjectDictionary()
        {
            var source = new PublicTwoFields<int, Dictionary<string, object>>
            {
                Value1 = 6372,
                Value2 = new Dictionary<string, object>
                {
                    ["QueryName"] = "References",
                    ["IsDefault"] = false,
                    ["QueryId"] = 155,
                    ["WorkspaceTypeId"] = 1,
                    ["IsUserDefined"] = true,
                    ["QueryTypeId"] = 2,
                    ["Test"] = default(int?)
                }
            };

            var result = source.DeepClone();

            result.Value1.ShouldBe(6372);
            result.Value2.ShouldNotBeNull();
            result.Value2.ShouldNotBeSameAs(source.Value2);
            result.Value2.Count.ShouldBe(source.Value2.Count);
            result.Value2.ShouldContainKeyAndValue("QueryName", "References");
            result.Value2.ShouldContainKeyAndValue("IsDefault", false);
            result.Value2.ShouldContainKeyAndValue("QueryId", 155);
            result.Value2.ShouldContainKeyAndValue("WorkspaceTypeId", 1);
            result.Value2.ShouldContainKeyAndValue("IsUserDefined", true);
            result.Value2.ShouldContainKeyAndValue("QueryTypeId", 2);
            result.Value2.ShouldContainKeyAndValue("Test", null);
        }

        [Fact]
        public void ShouldNotCreateDictionaryAsFallbackComplexType()
        {
            var source = new PublicReadOnlyProperty<IDictionary<string, string>>(
                new Dictionary<string, string>());

            var cloned = Mapper.DeepClone(source);

            cloned.ShouldBeNull();
        }

        [Fact]
        public void ShouldFlattenAComplexTypeCollectionToANestedObjectDictionaryImplementation()
        {
            var source = new PublicField<ICollection<Customer>>()
            {
                Value = new[]
                {
                    new Customer
                    {
                        Id = Guid.NewGuid(),
                        Title = Title.Count,
                        Name = "Customer 1",
                        Address = new Address
                        {
                            Line1 = "This place",
                            Line2 = "That place",
                        }
                    },
                    default(Customer),
                    new MysteryCustomer
                    {
                        Id = Guid.NewGuid(),
                        Title = Title.Dr,
                        Name = "Customer 2",
                        Discount = 0.3m,
                        Report = "It was all a mystery :o"
                    }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicField<StringKeyedDictionary<object>>>();

            result.Value.ShouldNotBeNull();

            result.Value["[0].Id"].ShouldBe(source.Value.First().Id);
            result.Value["[0].Title"].ShouldBe(Title.Count);
            result.Value["[0].Name"].ShouldBe("Customer 1");
            result.Value.ShouldNotContainKey("[0].Address");
            result.Value["[0].Address.Line1"].ShouldBe("This place");
            result.Value["[0].Address.Line2"].ShouldBe("That place");

            result.Value["[2].Id"].ShouldBe(source.Value.Third().Id);
            result.Value["[2].Title"].ShouldBe(Title.Dr);
            result.Value["[2].Name"].ShouldBe("Customer 2");
            result.Value["[2].Discount"].ShouldBe(0.3m);
            result.Value["[2].Report"].ShouldBe("It was all a mystery :o");
            result.Value.ShouldNotContainKey("[2].Address");
            result.Value.ShouldNotContainKey("[2].Address.Line1");
            result.Value.ShouldNotContainKey("[2].Address.Line2");

        }

        #region Helper Members

        private static class Issue97
        {
            public class ReadonlyDictionary
            {
                public ReadonlyDictionary()
                {
                    Dictionary = new Dictionary<string, string>();
                }

                public IDictionary<string, string> Dictionary { get; }
            }
        }

        #endregion
    }
}
