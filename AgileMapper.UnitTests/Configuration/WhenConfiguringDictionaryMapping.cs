namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDictionaryMapping
    {
        [Fact]
        public void ShouldUseACustomFullDictionaryKeyForARootMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<PublicField<string>>()
                    .MapKey("BoomDiddyBoom")
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
                    .MapKey("BoomDiddyMcBoom")
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
                    .MapMemberName("HouseNumber")
                    .To(a => a.Line1)
                    .And
                    .MapMemberName("StreetName")
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
                    .FromDictionaries
                    .ToANew<Address>()
                    .MapMemberName("HouseName")
                    .To(a => a.Line1);

                var source = new Dictionary<string, string> { ["Value.HouseName"] = "Home" };
                var result = mapper.Map(source).ToANew<PublicProperty<Address>>();

                result.Value.Line1.ShouldBe("Home");
            }
        }

        [Fact]
        public void ShouldApplyNonDictionarySpecificConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<Product>()
                    .MapKey("BlahBlahBlah")
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
        public void ShouldApplyFlattenedMemberNamesGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .UseFlattenedMemberNames();

                var source = new Dictionary<string, string>
                {
                    ["Name"] = "Bob",
                    ["Discount"] = "0.1",
                    ["AddressLine1"] = "Bob's House",
                    ["AddressLine2"] = "Bob's Street"
                };
                var result = mapper.Map(source).ToANew<Customer>();

                result.Name.ShouldBe("Bob");
                result.Discount.ShouldBe(0.1);
                result.Address.Line1.ShouldBe("Bob's House");
                result.Address.Line2.ShouldBe("Bob's Street");
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecifiedTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .To<Order>()
                    .UseFlattenedMemberNames()
                    .And
                    .MapMemberName("OrderCode")
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
                    .FromDictionaries
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
                    .MapKey("PersonName")
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
    }
}
