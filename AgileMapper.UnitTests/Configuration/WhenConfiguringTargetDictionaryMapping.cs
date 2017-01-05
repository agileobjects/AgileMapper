namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
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
    }
}