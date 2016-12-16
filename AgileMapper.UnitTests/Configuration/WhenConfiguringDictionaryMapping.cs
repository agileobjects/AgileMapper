namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
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
                    .FromDictionaries()
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
                    .FromDictionaries()
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
                    .FromDictionaries()
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
                    .FromDictionaries()
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
                    .FromDictionaries()
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
        public void ShouldApplyFlattenedMemberNamesIfConfigured()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries()
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
    }
}
