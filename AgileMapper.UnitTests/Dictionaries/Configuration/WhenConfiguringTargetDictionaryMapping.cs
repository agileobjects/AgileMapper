namespace AgileObjects.AgileMapper.UnitTests.Dictionaries.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
                    .UseFlattenedTargetMemberNames();

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
        public void ShouldNotApplySourceOnlyConfigurationToTargetDictionaries()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries
                    .UseFlattenedTargetMemberNames();

                var source = new Customer
                {
                    Name = "Paul",
                    Address = new Address { Line1 = "Abbey Road", Line2 = "Penny Lane" }
                };
                var result = mapper.Map(source).ToANew<Dictionary<string, string>>();

                result["Name"].ShouldBe("Paul");
                result["Address.Line1"].ShouldBe("Abbey Road");
                result["Address.Line2"].ShouldBe("Penny Lane");
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecificSourceType()
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
        public void ShouldApplyACustomSeparatorToASpecificSourceType()
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
                matchingResult.ShouldNotContainKey("Address.Line1");
                matchingResult.ShouldNotContainKey("Address!Line1");

                var nonMatchingSource = new Customer { Address = address };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<Dictionary<string, string>>();

                nonMatchingSourceResult["Address.Line1"].ShouldBe("Paddy's");

                var nonMatchingTargetResult = mapper.Map(nonMatchingSource).ToANew<Dictionary<string, object>>();

                nonMatchingTargetResult["Address.Line1"].ShouldBe("Paddy's");
            }
        }

        [Fact]
        public void ShouldApplyACustomEnumerableElementPatternGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dictionaries
                    .UseMemberNameSeparator("-")
                    .UseElementKeyPattern("+i+");

                var source = new[]
                {
                    new Address { Line1 = "Mato's House", Line2 = "Mato's Street" },
                    new Address { Line1 = "Magnus's House", Line2 = "Magnus's Street" },
                };
                var result = mapper.Map(source).ToANew<Dictionary<string, object>>();

                result.Count.ShouldBe(4);
                result["+0+-Line1"].ShouldBe("Mato's House");
                result["+0+-Line2"].ShouldBe("Mato's Street");
                result["+1+-Line1"].ShouldBe("Magnus's House");
                result["+1+-Line2"].ShouldBe("Magnus's Street");
            }
        }

        [Fact]
        public void ShouldApplyACustomEnumerableElementPatternToASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .ToDictionaries
                    .UseElementKeyPattern("(i)");

                var source = new PublicProperty<IEnumerable<Person>>
                {
                    Value = new Collection<Person>
                    {
                        new Person { Name = "Sandra", Address = new Address { Line1 = "Home" } },
                        new Person { Name = "David", Address = new Address { Line1 = "Home!" } }
                    }
                };
                var target = new PublicField<IDictionary<string, object>>
                {
                    Value = new Dictionary<string, object>()
                };
                var originalDictionary = target.Value;

                mapper.Map(source).OnTo(target);

                target.Value.ShouldBeSameAs(originalDictionary);

                target.Value["(0).Name"].ShouldBe("Sandra");
                target.Value["(0).Address.Line1"].ShouldBe("Home");

                target.Value["(1).Name"].ShouldBe("David");
                target.Value["(1).Address.Line1"].ShouldBe("Home!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredConditionalTargetEntryValue()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomerViewModel>()
                    .ToDictionaries
                    .If((mcvm, d) => mcvm.Discount > 0.5)
                    .Map((mcvm, d) => mcvm.Name + " (Big discount!)")
                    .To(d => d["Name"]);

                var noDiscountSource = new MysteryCustomerViewModel { Name = "Schumer", Discount = 0.0 };
                var noDiscountResult = mapper.Map(noDiscountSource).ToANew<Dictionary<string, object>>();

                noDiscountResult["Name"].ShouldBe("Schumer");

                var bigDiscountSource = new MysteryCustomerViewModel { Name = "Silverman", Discount = 0.6 };
                var bigDiscountResult = mapper.Map(bigDiscountSource).ToANew<Dictionary<string, object>>();

                bigDiscountResult["Name"].ShouldBe("Silverman (Big discount!)");
            }
        }

        [Fact]
        public void ShouldAllowACustomTargetEntryKey()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomerViewModel>()
                    .ToDictionaries
                    .MapMember(mcvm => mcvm.Name)
                    .ToFullKey("CustomerName")
                    .And
                    .If((mcvm, d) => mcvm.Discount > 0.5)
                    .Map((mcvm, d) => mcvm.Name + " (Big discount!)")
                    .To(d => d["Name"]);

                var noDiscountSource = new MysteryCustomerViewModel { Name = "Schumer", Discount = 0.0 };
                var noDiscountResult = mapper.Map(noDiscountSource).ToANew<Dictionary<string, object>>();

                noDiscountResult["CustomerName"].ShouldBe("Schumer");
                noDiscountResult.ShouldNotContainKey("Name");

                var bigDiscountSource = new MysteryCustomerViewModel { Name = "Silverman", Discount = 0.6 };
                var bigDiscountResult = mapper.Map(bigDiscountSource).ToANew<Dictionary<string, object>>();

                bigDiscountResult["CustomerName"].ShouldBe("Silverman");
                bigDiscountResult["Name"].ShouldBe("Silverman (Big discount!)");
            }
        }

        [Fact]
        public void ShouldApplyACustomConfiguredMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address>()
                    .ToDictionaries
                    .If(ctx => !string.IsNullOrEmpty(ctx.Source.Line2))
                    .Map(ctx => "Present")
                    .To(d => d["Line2State"])
                    .But
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .Map(ctx => "Missing")
                    .To(d => d["Line2State"]);

                var line2Source = new Address { Line1 = "Line 1: Yes", Line2 = "Line 2: Yes!" };
                var line2Result = mapper.Map(line2Source).ToANew<Dictionary<string, object>>();

                line2Result["Line2State"].ShouldBe("Present");
                line2Result["Line2"].ShouldBe("Line 2: Yes!");

                var noLine2Source = new Address { Line1 = "Line 1: Yes", Line2 = string.Empty };
                var noLine2Result = mapper.Map(noLine2Source).ToANew<Dictionary<string, object>>();

                noLine2Result["Line2State"].ShouldBe("Missing");
            }
        }
    }
}