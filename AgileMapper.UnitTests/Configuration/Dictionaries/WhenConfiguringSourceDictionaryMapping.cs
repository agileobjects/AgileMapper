namespace AgileObjects.AgileMapper.UnitTests.Configuration.Dictionaries
{
    using System.Collections.Generic;
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
    public class WhenConfiguringSourceDictionaryMapping
    {
        [Fact]
        public void ShouldUseACustomFullDictionaryKeyForARootMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<PublicField<string>>()
                    .MapFullKey("BoomDiddyBoom")
                    .To(pf => pf.Value);

                var source = new Dictionary<string, int> { ["BoomDiddyBoom"] = 123 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("123");
            }
        }

        [Fact]
        public void ShouldUseACustomFullDictionaryKeyForANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .OnTo<PublicField<PublicProperty<decimal>>>()
                    .MapFullKey("BoomDiddyMcBoom")
                    .To(pf => pf.Value.Value);

                var source = new Dictionary<string, string> { ["BoomDiddyMcBoom"] = "6476338" };
                var target = new PublicField<PublicProperty<decimal>>
                {
                    Value = new PublicProperty<decimal>()
                };
                var result = mapper.Map(source).OnTo(target);

                result.Value.Value.ShouldBe(6476338.00m);
            }
        }

        [Fact]
        public void ShouldUseCustomMemberNameDictionaryKeysForRootMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .Over<Address>()
                    .MapMemberNameKey("HouseNumber")
                    .To(a => a.Line1)
                    .And
                    .MapMemberNameKey("StreetName")
                    .To(a => a.Line2);

                var source = new Dictionary<string, string>
                {
                    ["HouseNumber"] = "123",
                    ["StreetName"] = "Street Road"
                };
                var target = new Address { Line1 = "Unknown", Line2 = "Unknown" };
                var result = mapper.Map(source).Over(target);

                result.Line1.ShouldBe("123");
                result.Line2.ShouldBe("Street Road");
            }
        }

        [Fact]
        public void ShouldUseACustomMemberNameDictionaryKeyForANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<string>()
                    .ToANew<Address>()
                    .MapMemberNameKey("HouseName")
                    .To(a => a.Line1);

                var source = new Dictionary<string, string> { ["Value.HouseName"] = "Home" };
                var result = mapper.Map(source).ToANew<PublicProperty<Address>>();

                result.Value.Line1.ShouldBe("Home");
            }
        }

        [Fact]
        public void ShouldUseACustomMemberNameDictionaryKeyForANestedArrayMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<PublicProperty<string[]>>()
                    .MapMemberNameKey("Strings")
                    .To(pp => pp.Value);

                var source = new Dictionary<string, string>
                {
                    ["Strings[0]"] = "Zero",
                    ["Strings[1]"] = "One",
                    ["Strings[2]"] = "Two"
                };
                var result = mapper.Map(source).ToANew<PublicProperty<string[]>>();

                result.Value.Length.ShouldBe(3);
                result.Value.First().ShouldBe("Zero");
                result.Value.Second().ShouldBe("One");
                result.Value.Third().ShouldBe("Two");
            }
        }

        [Fact]
        public void ShouldApplyNonDictionarySpecificConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<object>()
                    .To<Product>()
                    .MapFullKey("BlahBlahBlah")
                    .To(p => p.ProductId)
                    .And
                    .If(ctx => ctx.Source.Count > 2)
                    .Ignore(p => p.Price)
                    .But
                    .If((d, p) => d.Count < 2)
                    .Map((d, p, i) => d.Count)
                    .To(p => p.Price);

                var oneEntrySource = new Dictionary<string, object>
                {
                    ["BlahBlahBlah"] = "Dictionary Madness!"
                };
                var oneEntryResult = mapper.Map(oneEntrySource).ToANew<Product>();

                oneEntryResult.ProductId.ShouldBe("Dictionary Madness!");
                oneEntryResult.Price.ShouldBe(1.00);

                var twoEntriesSource = new Dictionary<string, object>
                {
                    ["BlahBlahBlah"] = "DictionaryAdventures.com",
                    ["Price"] = 123.00
                };
                var twoEntriesResult = mapper.Map(twoEntriesSource).ToANew<Product>();

                twoEntriesResult.ProductId.ShouldBe("DictionaryAdventures.com");
                twoEntriesResult.Price.ShouldBe(123.00);

                var threeEntriesSource = new Dictionary<string, object>
                {
                    ["BlahBlahBlah"] = "Dictionary Madness!!!",
                    ["Price"] = 123.00,
                    ["ExtraEntry"] = "Taking up space"
                };
                var threeEntriesResult = mapper.Map(threeEntriesSource).ToANew<Product>();

                threeEntriesResult.ProductId.ShouldBe("Dictionary Madness!!!");
                threeEntriesResult.Price.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<Order>()
                    .MapMemberNameKey("OrderCode")
                    .To(o => o.OrderId);

                var source = new Dictionary<string, object>
                {
                    ["OrderCode"] = 64673438,
                    ["Items[0]OrderItemId"] = 123,
                    ["Items[0]ProductID"] = "XYZ",
                    ["Items[1]OrderItemId"] = 987,
                    ["Items[1]ProductID"] = "ABC"
                };
                var result = mapper.Map(source).ToANew<Order>();

                result.OrderId.ShouldBe(64673438);

                result.Items.First().OrderItemId.ShouldBe(123);
                result.Items.First().ProductId.ShouldBe("XYZ");

                result.Items.Second().OrderItemId.ShouldBe(987);
                result.Items.Second().ProductId.ShouldBe("ABC");
            }
        }

        [Fact]
        public void ShouldApplyACustomSeparatorGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dictionaries
                    .UseMemberNameSeparator("-");

                var source = new Dictionary<string, object>
                {
                    ["Name"] = "Jimmy",
                    ["Address-Line1"] = "Jimmy's House",
                    ["Address-Line2"] = "Jimmy's Street"
                };
                var target = new MysteryCustomer { Address = new Address { Line1 = "La la la" } };
                var result = mapper.Map(source).OnTo(target);

                result.Name.ShouldBe("Jimmy");
                result.Address.Line1.ShouldBe("La la la");
                result.Address.Line2.ShouldBe("Jimmy's Street");
            }
        }

        [Fact]
        public void ShouldApplyACustomSeparatorToASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .ToANew<Customer>()
                    .UseMemberNameSeparator("_")
                    .And
                    .MapFullKey("PersonName")
                    .To(p => p.Name);

                var source = new Dictionary<string, object>
                {
                    ["Name"] = "Freddy",
                    ["PersonName"] = "Bobby",
                    ["Discount"] = 0.3,
                    ["Address.Line1"] = "Freddy's Dot",
                    ["Address_Line1"] = "Bobby's Underscore"
                };

                var nonMatchingResult = mapper.Map(source).ToANew<Person>();

                nonMatchingResult.Name.ShouldBe("Freddy");
                nonMatchingResult.Address.Line1.ShouldBe("Freddy's Dot");

                var matchingResult = mapper.Map(source).ToANew<Customer>();

                matchingResult.Name.ShouldBe("Bobby");
                matchingResult.Discount.ShouldBe(0.3);
                matchingResult.Address.Line1.ShouldBe("Bobby's Underscore");
            }
        }

        [Fact]
        public void ShouldApplyGlobalTypeSpecificCustomSeparators()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dictionaries
                    .UseMemberNameSeparator("+")
                    .AndWhenMapping
                    .FromDictionaries
                    .ToANew<Address>()
                    .UseMemberNameSeparator("-");

                var source = new Dictionary<string, object>
                {
                    ["[0]+Name"] = "Elizabeth",
                    ["[0]-Address-Line1"] = "Buck Palace",
                    ["[0]-Address-Line2"] = "London"
                };

                var result = mapper.Map(source).ToANew<Person[]>();

                result.ShouldHaveSingleItem();

                result.First().Name.ShouldBe("Elizabeth");
                result.First().Address.Line1.ShouldBe("Buck Palace");
                result.First().Address.Line2.ShouldBe("London");
            }
        }

        [Fact]
        public void ShouldApplyACustomEnumerableElementPatternGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .UseElementKeyPattern("_i_")
                    .AndWhenMapping
                    .To<PublicSetMethod<string>>()
                    .MapMemberNameKey("Value")
                    .To<string>(psm => psm.SetValue);

                var source = new Dictionary<string, string>
                {
                    ["_0_Value"] = "blah",
                    ["_1_Value"] = "bleh",
                    ["_2_Value"] = "bluh"
                };

                var target = new List<PublicSetMethod<string>>();
                var result = mapper.Map(source).Over(target);

                result.Count.ShouldBe(3);
                result.First().Value.ShouldBe("blah");
                result.Second().Value.ShouldBe("bleh");
                result.Third().Value.ShouldBe("bluh");
            }
        }

        [Fact]
        public void ShouldApplyACustomEnumerableElementPatternToASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .DictionariesWithValueType<string>()
                    .UseMemberNameSeparator("-")
                    .UseElementKeyPattern("i")
                    .AndWhenMapping
                    .FromDictionariesWithValueType<string>()
                    .OnTo<Address>()
                    .MapMemberNameKey("StreetName")
                    .To(a => a.Line1)
                    .And
                    .MapMemberNameKey("CityName")
                    .To(a => a.Line2);

                var source = new Dictionary<string, string>
                {
                    ["Value0-StreetName"] = "Street Zero",
                    ["Value0-CityName"] = "City Zero",
                    ["Value1-StreetName"] = "Street One",
                    ["Value1-CityName"] = "City One"
                };

                var target = new PublicField<IEnumerable<Address>> { Value = new List<Address>() };
                var result = mapper.Map(source).OnTo(target);

                result.Value.Count().ShouldBe(2);

                result.Value.First().Line1.ShouldBe("Street Zero");
                result.Value.First().Line2.ShouldBe("City Zero");

                result.Value.Second().Line1.ShouldBe("Street One");
                result.Value.Second().Line2.ShouldBe("City One");
            }
        }

        [Fact]
        public void ShouldMapToANestedSimpleTypeListWithACustomElementSeparator()
        {
            var source = new Dictionary<string, string>
            {
                ["Value_0_"] = "abc",
                ["Value_1_"] = "xyz",
                ["Value_2_"] = "123"
            };

            var result = Mapper.Map(source).ToANew<PublicField<IList<string>>>(cfg => cfg
                .WhenMapping
                .FromDictionaries
                .ToANew<IList<string>>()
                .UseElementKeyPattern("_i_"));

            result.Value.Count.ShouldBe(3);
            result.Value.First().ShouldBe("abc");
            result.Value.Second().ShouldBe("xyz");
            result.Value.Third().ShouldBe("123");
        }

        [Fact]
        public void ShouldApplyACustomSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .ToANew<PublicField<long>>()
                    .Map(ctx => ctx.Source.Count)
                    .To(pf => pf.Value);

                var source = new Dictionary<string, object> { ["One"] = 1, ["Two"] = 2 };
                var result = mapper.Map(source).ToANew<PublicField<long>>();

                result.Value.ShouldBe(2);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/162
        [Fact]
        public void ShouldApplyAnElementKey()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicTwoFields<int, string>>()
                    .Map(s => s.Value, ptf => ptf.Value1)
                    .And
                    .Map(ctx => ctx.ElementKey)
                    .To(ptf => ptf.Value2);

                var source = new Dictionary<string, PublicField<int>>
                {
                    ["One"] = new PublicField<int> { Value = 1 },
                    ["Two"] = new PublicField<int> { Value = 2 }
                };

                var result = mapper.Map(source).ToANew<Dictionary<string, PublicTwoFields<int, string>>>();

                result.Count.ShouldBe(2);

                var result1 = result.ShouldContainKey("One")["One"];
                result1.Value1.ShouldBe(1);
                result1.Value2.ShouldBe("One");

                var result2 = result.ShouldContainKey("Two")["Two"];
                result2.Value1.ShouldBe(2);
                result2.Value2.ShouldBe("Two");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/162
        [Fact]
        public void ShouldApplyAnElementKeyToANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicTwoFields<long, PublicProperty<string>>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(ptf => ptf.Value1)
                    .And
                    .Map(ctx => ctx.ElementKey)
                    .To(ptf => ptf.Value2.Value);

                var source = new Dictionary<string, PublicField<int>>
                {
                    ["111"] = new PublicField<int> { Value = 123 }
                };

                var result = mapper.Map(source).ToANew<Dictionary<int, PublicTwoFields<long, PublicProperty<string>>>>();

                result.ShouldHaveSingleItem();

                var result1 = result.ShouldContainKey(111)[111];
                result1.Value1.ShouldBe(246);
                result1.Value2.ShouldNotBeNull();
                result1.Value2.Value.ShouldBe("111");
            }
        }

        [Fact]
        public void ShouldMapAnElementKeyToAComplexTypeCollectionFromTypedEntries()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MegaProduct>()
                    .To<MegaProduct>()
                    .Map(ctx => ctx.Source.ProductId + ctx.ElementKey)
                    .To(mp => mp.ProductId);

                var source = new Dictionary<string, MegaProduct>
                {
                    ["[0]"] = new MegaProduct { ProductId = "iyrfd" },
                    ["[1]"] = new MegaProduct { ProductId = "4r6sf" }
                };
                var result = mapper.Map(source).ToANew<ICollection<MegaProduct>>();

                result.Count.ShouldBe(2);
                result.First().ProductId.ShouldBe("iyrfd[0]");
                result.Second().ProductId.ShouldBe("4r6sf[1]");
            }
        }

        [Fact]
        public void ShouldConditionallyMapToDerivedTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .ToANew<PersonViewModel>()
                    .If(s => s.Source.ContainsKey("Discount"))
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(s => s.Source.ContainsKey("Report"))
                    .MapTo<MysteryCustomerViewModel>();

                var source = new Dictionary<string, object> { ["Name"] = "Person" };
                var personResult = mapper.Map(source).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Person");

                source.Add("Discount", "0.05");
                var customerResult = mapper.Map(source).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                ((CustomerViewModel)customerResult).Discount.ShouldBe(0.05);

                source.Add("Report", "Very good!");
                var mysteryCustomerResult = mapper.Map(source).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Very good!");
            }
        }

        [Fact]
        public void ShouldConditionallyMapToDerivedTypesFromASpecificValueTypeDictionary()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<string>()
                    .ToANew<CustomerViewModel>()
                    .If(s => s.Source["Report"].Length > 10)
                    .MapTo<MysteryCustomerViewModel>();

                var source = new Dictionary<string, string>
                {
                    ["Name"] = "Customer",
                    ["Report"] = "Too short!"
                };
                var customerResult = mapper.Map(source).ToANew<CustomerViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                customerResult.Name.ShouldBe("Customer");

                source["Report"] = "Plenty long enough!";
                var mysteryCustomerResult = mapper.Map(source).ToANew<CustomerViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                mysteryCustomerResult.Name.ShouldBe("Customer");
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Plenty long enough!");
            }
        }

        [Fact]
        public void ShouldRestrictCustomKeysByDictionaryValueType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<string>()
                    .ToANew<PublicPropertyStruct<string>>()
                    .MapFullKey("LaLaLa")
                    .To(p => p.Value);

                var matchingSource = new Dictionary<string, string>
                {
                    ["LaLaLa"] = "1",
                    ["Value"] = "2"
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicPropertyStruct<string>>();

                matchingResult.Value.ShouldBe("1");

                var nonMatchingSource = new Dictionary<string, object>
                {
                    ["LaLaLa"] = "20",
                    ["Value"] = "10"
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicPropertyStruct<string>>();

                nonMatchingResult.Value.ShouldBe("10");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/152
        [Fact]
        public void ShouldMapANestedComplexTypeSourceDictionaryToTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue152.Source.Wrapper>().ToDictionariesWithValueType<Issue152.Target.Data>()
                    .Map(ctx => ctx.Source.Dict).ToTarget()
                    .And
                    .IgnoreTargetMembersWhere(m => m.Name == "Dict");

                var source = new Issue152.Source.Wrapper("One", "Two", "Three");
                var result = mapper.Map(source).ToANew<IDictionary<string, Issue152.Target.Data>>();

                result.ShouldNotBeNull();
                result.Count.ShouldBe(3);

                result.ShouldContainKey("One");
                result["One"].Value.ShouldBe("One");

                result.ShouldContainKey("Two");
                result["Two"].Value.ShouldBe("Two");

                result.ShouldContainKey("Three");
                result["Three"].Value.ShouldBe("Three");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/173
        [Fact]
        public void ShouldMapDictionaryComplexTypeValuesToANestedToTargetList()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<PublicField<string>>()
                    .ToANew<List<PublicField<string>>>()
                    .Map((src, _) => src.Values.ToList()).ToTarget();

                var source = new PublicProperty<Dictionary<string, PublicField<string>>>
                {
                    Value = new Dictionary<string, PublicField<string>>
                    {
                        ["test1"] = new PublicField<string> { Value = "value1" },
                        ["test2"] = new PublicField<string> { Value = "value2" }
                    }
                };

                var result = mapper.Map(source).ToANew<PublicField<List<PublicField<string>>>>();

                result.Value.Count.ShouldBe(2);
                result.Value.First().Value.ShouldBe("value1");
                result.Value.Second().Value.ShouldBe("value2");
            }
        }

        #region Helper Classes

        public static class Issue152
        {
            public static class Source
            {
                public class Wrapper
                {
                    public Wrapper(params string[] inputs)
                    {
                        Dict = new Dictionary<string, Data>();

                        foreach (var stringValue in inputs)
                        {
                            Dict.Add(stringValue, new Data { Value = stringValue });
                        }
                    }

                    public IDictionary<string, Data> Dict { get; }
                }

                public class Data
                {
                    public string Value { get; set; }
                }
            }

            public static class Target
            {
                public class Data
                {
                    public string Value { get; set; }
                }
            }
        }

        #endregion
    }
}
