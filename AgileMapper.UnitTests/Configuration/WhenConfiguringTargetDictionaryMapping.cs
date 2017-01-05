namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringTargetDictionaryMapping
    {
        [Fact]
        public void ShouldApplyFlattenedMemberNamesGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dictionaries
                    .UseFlattenedMemberNames();

                var source = new MysteryCustomer
                {
                    Id = Guid.NewGuid(),
                    Title = Title.Mr,
                    Name = "Paul",
                    Discount = 0.25m,
                    Report = "Naah nah nah na-na-na NAAAAAAAHHHH",
                    Address = new Address { Line1 = "Abbey Road", Line2 = "Penny Lane" }
                };
                var result = mapper.Map(source).ToANew<Dictionary<string, string>>();

                result["Id"].ShouldBe(source.Id.ToString());
                result["Title"].ShouldBe("Mr");
                result["Name"].ShouldBe("Paul");
                result["Discount"].ShouldBe("0.25");
                result["AddressLine1"].ShouldBe("Abbey Road");
                result["AddressLine2"].ShouldBe("Penny Lane");
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecifiedSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .ToDictionaries
                    .UseFlattenedMemberNames()
                    .And
                    .MapMember(pf => pf.Value)
                    .ToMemberNameKey("Data");

                var matchingSource = new PublicField<Address>
                {
                    Value = new Address { Line1 = "As a pancake" }
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<Dictionary<string, object>>();

                matchingResult.Keys.Any(k => k.StartsWith("Value")).ShouldBeFalse();
                matchingResult["DataLine1"].ShouldBe("As a pancake");
                matchingResult["DataLine2"].ShouldBeNull();

                var nonMatchingSource = new PublicProperty<Address>
                {
                    Value = new Address { Line1 = "Like a flatfish" }
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Dictionary<string, object>>();

                nonMatchingResult["Value.Line1"].ShouldBe("Like a flatfish");
                nonMatchingResult["Value.Line2"].ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyACustomSeparatorGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dictionaries
                    .UseMemberNameSeparator("_");

                var source = new Person
                {
                    Name = "Jenny",
                    Address = new Address
                    {
                        Line1 = "Jenny's House",
                        Line2 = "Jenny's Street"
                    }
                };
                var target = new Dictionary<string, object>
                {
                    ["Address.Line1"] = "Jenny's Apartment"
                };
                var result = mapper.Map(source).OnTo(target);

                result["Name"].ShouldBe("Jenny");
                result["Address.Line1"].ShouldBe("Jenny's Apartment");
                result["Address_Line1"].ShouldBe("Jenny's House");
                result["Address_Line2"].ShouldBe("Jenny's Street");
            }
        }

        [Fact]
        public void ShouldApplyACustomSeparatorToASpecifiedSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .ToDictionariesWithValueType<string>()
                    .UseMemberNameSeparator("!")
                    .And
                    .MapMember(c => c.Address.Line1)
                    .ToFullKey("StreetAddress");

                var address = new Address { Line1 = "Paddy's", Line2 = "Philly" };
                var matchingSource = new MysteryCustomer { Address = address };
                var matchingResult = mapper.Map(matchingSource).ToANew<Dictionary<string, string>>();

                matchingResult["StreetAddress"].ShouldBe("Paddy's");
                matchingResult["Address!Line2"].ShouldBe("Philly");
                matchingResult.ContainsKey("Address.Line1").ShouldBeFalse();
                matchingResult.ContainsKey("Address!Line1").ShouldBeFalse();

                var nonMatchingSource = new Customer { Address = address };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<Dictionary<string, string>>();

                nonMatchingSourceResult["Address.Line1"].ShouldBe("Paddy's");

                var nonMatchingTargetResult = mapper.Map(nonMatchingSource).ToANew<Dictionary<string, object>>();

                nonMatchingTargetResult["Address.Line1"].ShouldBe("Paddy's");
            }
        }
    }
}