namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Common;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringTargetDynamicMapping
    {
        [Fact]
        public void ShouldApplyFlattenedMemberNamesGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dynamics
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
                var result = mapper.Map(source).ToANew<dynamic>();

                ((Guid)result.Id).ShouldBe(source.Id);
                ((Title)result.Title).ShouldBe(Title.Mr);
                ((string)result.Name).ShouldBe("Paul");
                ((decimal)result.Discount).ShouldBe(0.25m);
                ((string)result.AddressLine1).ShouldBe("Abbey Road");
                ((string)result.AddressLine2).ShouldBe("Penny Lane");
            }
        }

        [Fact]
        public void ShouldNotApplySourceOnlyConfigurationToTargetDynamics()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .UseFlattenedTargetMemberNames();

                var source = new Customer
                {
                    Name = "Paul",
                    Address = new Address { Line1 = "Abbey Road", Line2 = "Penny Lane" }
                };

                dynamic target = new ExpandoObject();

                target.Name = "Ringo";

                mapper.Map(source).OnTo(target);

                ((string)target.Name).ShouldBe("Ringo");
                ((string)target.Address_Line1).ShouldBe("Abbey Road");
                ((string)target.Address_Line2).ShouldBe("Penny Lane");
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecificSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .ToDynamics
                    .UseFlattenedMemberNames()
                    .And
                    .MapMember(pf => pf.Value)
                    .ToMemberName("Data");

                var matchingSource = new PublicField<Address>
                {
                    Value = new Address { Line1 = "As a pancake" }
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<dynamic>();

                ((IDictionary<string, object>)matchingResult).Keys.Any(k => k.StartsWith("Value")).ShouldBeFalse();
                ((string)matchingResult.DataLine1).ShouldBe("As a pancake");
                ((string)matchingResult.DataLine2).ShouldBeNull();

                var nonMatchingSource = new PublicProperty<Address>
                {
                    Value = new Address { Line1 = "Like a flatfish" }
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<dynamic>();

                ((string)nonMatchingResult.Value_Line1).ShouldBe("Like a flatfish");
                ((string)nonMatchingResult.Value_Line2).ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyACustomSeparatorToASpecificSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .ToDynamics
                    .UseMemberNameSeparator("!")
                    .And
                    .MapMember(c => c.Address.Line1)
                    .ToFullMemberName("StreetAddress");

                var address = new Address { Line1 = "Paddy's", Line2 = "Philly" };
                var matchingSource = new MysteryCustomer { Address = address };
                var matchingResult = (IDictionary<string, object>)mapper.Map(matchingSource).ToANew<dynamic>();

                matchingResult["StreetAddress"].ShouldBe("Paddy's");
                matchingResult["Address!Line2"].ShouldBe("Philly");
                matchingResult.ShouldNotContainKey("Address_Line1");
                matchingResult.ShouldNotContainKey("Address!Line1");

                var nonMatchingSource = new Customer { Address = address };
                var nonMatchingSourceResult = (IDictionary<string, object>)mapper.Map(nonMatchingSource).ToANew<dynamic>();

                nonMatchingSourceResult["Address_Line1"].ShouldBe("Paddy's");

                var nonMatchingTargetResult = (IDictionary<string, object>)mapper.Map(nonMatchingSource).ToANew<dynamic>();

                nonMatchingTargetResult["Address_Line1"].ShouldBe("Paddy's");
            }
        }

        [Fact]
        public void ShouldApplyACustomEnumerableElementPatternToASpecificDerivedSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .ToDynamics
                    .UseElementKeyPattern("(i)");

                var source = new PublicProperty<IEnumerable<Person>>
                {
                    Value = new List<Person>
                    {
                        new Person { Name = "Sandra", Address = new Address { Line1 = "Home" } },
                        new Customer { Name = "David", Address = new Address { Line1 = "Home!" } }
                    }
                };
                var originalExpando = new ExpandoObject();
                var target = new PublicField<dynamic> { Value = originalExpando };

                var result = (IDictionary<string, object>)mapper.Map(source).OnTo(target).Value;

                result.ShouldBeSameAs(originalExpando);

                result["_0_Name"].ShouldBe("Sandra");
                result["_0_Address_Line1"].ShouldBe("Home");

                result["(1)_Name"].ShouldBe("David");
                result["(1)_Address_Line1"].ShouldBe("Home!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredConditionalTargetEntryValue()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomerViewModel>()
                    .ToDynamics
                    .If((mcvm, d) => mcvm.Discount > 0.5)
                    .Map((mcvm, d) => mcvm.Name + " (Big discount!)")
                    .To(d => d["Name"]);

                var noDiscountSource = new MysteryCustomerViewModel { Name = "Schumer", Discount = 0.0 };
                var noDiscountResult = (IDictionary<string, object>)mapper.Map(noDiscountSource).ToANew<dynamic>();

                noDiscountResult["Name"].ShouldBe("Schumer");

                var bigDiscountSource = new MysteryCustomerViewModel { Name = "Silverman", Discount = 0.6 };
                var bigDiscountResult = (IDictionary<string, object>)mapper.Map(bigDiscountSource).ToANew<dynamic>();

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
                    .ToDynamics
                    .MapMember(mcvm => mcvm.Name)
                    .ToFullMemberName("CustomerName")
                    .And
                    .If((mcvm, d) => mcvm.Discount > 0.5)
                    .Map((mcvm, d) => mcvm.Name + " (Big discount!)")
                    .To(d => d["Name"]);

                var noDiscountSource = new MysteryCustomerViewModel { Name = "Schumer", Discount = 0.0 };
                var noDiscountResult = (IDictionary<string, object>)mapper.Map(noDiscountSource).ToANew<dynamic>();

                noDiscountResult["CustomerName"].ShouldBe("Schumer");
                noDiscountResult.ShouldNotContainKey("Name");

                var bigDiscountSource = new MysteryCustomerViewModel { Name = "Silverman", Discount = 0.6 };
                var bigDiscountResult = (IDictionary<string, object>)mapper.Map(bigDiscountSource).ToANew<dynamic>();

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
                    .ToDynamics
                    .If(ctx => !string.IsNullOrEmpty(ctx.Source.Line2))
                    .Map(ctx => "Present")
                    .To(d => d["Line2State"])
                    .But
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .Map(ctx => "Missing")
                    .To(d => d["Line2State"]);

                var line2Source = new Address { Line1 = "Line 1: Yes", Line2 = "Line 2: Yes!" };
                var line2Result = (IDictionary<string, object>)mapper.Map(line2Source).ToANew<dynamic>();

                line2Result["Line2State"].ShouldBe("Present");
                line2Result["Line2"].ShouldBe("Line 2: Yes!");

                var noLine2Source = new Address { Line1 = "Line 1: Yes", Line2 = string.Empty };
                var noLine2Result = (IDictionary<string, object>)mapper.Map(noLine2Source).ToANew<dynamic>();

                noLine2Result["Line2State"].ShouldBe("Missing");
            }
        }

        [Fact]
        public void ShouldNotApplyTargetDictionaryConfigurationToDynamics()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .ToDictionaries
                    .Map((pf, d) => pf.Value)
                    .To(d => d["Name"]);

                var source = new PublicField<string> { Value = "LaLaLa" };

                dynamic target = new ExpandoObject();

                target.Name = "Doodeedoo";

                mapper.Map(source).Over(target);

                ((object)target).ShouldNotBeNull();
                ((string)target.Value).ShouldBe("LaLaLa");
                ((string)target.Name).ShouldBe("Doodeedoo");
            }
        }

        [Fact]
        public void ShouldNotApplyTargetDynamicConfigurationToDictionaries()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToDynamics
                    .Map(ctx => ctx.Source.Value)
                    .To(d => d["Name"]);

                var source = new PublicField<int>
                {
                    Value = 123
                };

                var target = new Dictionary<string, int>
                {
                    ["Name"] = 1,
                    ["Value"] = 2
                };

                mapper.Map(source).Over(target);

                target["Name"].ShouldBe(1);
                target["Value"].ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldNotConflictTargetDynamicAndDictionaryConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Address>>()
                    .ToDictionaries
                    .UseMemberNameSeparator("-");

                mapper.WhenMapping
                    .From<PublicProperty<Address>>()
                    .ToDynamics
                    .UseMemberNameSeparator("+");

                var source = new PublicProperty<Address>
                {
                    Value = new Address { Line1 = "L1", Line2 = "L2" }
                };

                var dictionaryTarget = new Dictionary<string, object>
                {
                    ["Value-Line1"] = "Line 1!",
                    ["Value-Line2"] = "Line 2!"
                };

                mapper.Map(source).Over(dictionaryTarget);

                dictionaryTarget["Value-Line1"].ShouldBe("L1");
                dictionaryTarget["Value-Line2"].ShouldBe("L2");

                dynamic dynamicTarget = new ExpandoObject();
                var targetDynamicDictionary = (IDictionary<string, object>)dynamicTarget;

                targetDynamicDictionary["Value+Line1"] = "Line 1?!";
                targetDynamicDictionary["Value+Line2"] = "Line 2?!";

                mapper.Map(source).Over(dynamicTarget);

                targetDynamicDictionary["Value+Line1"].ShouldBe("L1");
                targetDynamicDictionary["Value+Line2"].ShouldBe("L2");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredRootSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .ToDynamics
                    .Map((pf, d) => pf.Value)
                    .ToTarget();

                var source = new PublicField<Address>
                {
                    Value = new Address { Line1 = "There!", Line2 = "There too!" }
                };

                var result = mapper.Map(source).ToANew<dynamic>();

                ((string)result.Value_Line1).ShouldBe("There!");
                ((string)result.Value_Line2).ShouldBe("There too!");
                ((string)result.Line1).ShouldBe("There!");
                ((string)result.Line2).ShouldBe("There too!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredRootSourceToANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .ToDynamics
                    .Map((pf, d) => pf.Value)
                    .ToTarget();

                var source = new PublicProperty<PublicField<Address>>
                {
                    Value = new PublicField<Address>
                    {
                        Value = new Address { Line1 = "Where?!", Line2 = "Here too!" }
                    }
                };

                var result = mapper.Map(source).ToANew<PublicField<ExpandoObject>>();

                var targetDynamicDictionary = (IDictionary<string, object>)result.Value;
                targetDynamicDictionary["Value_Line1"].ShouldBe("Where?!");
                targetDynamicDictionary["Value_Line2"].ShouldBe("Here too!");
                targetDynamicDictionary["Line1"].ShouldBe("Where?!");
                targetDynamicDictionary["Line2"].ShouldBe("Here too!");
            }
        }
    }
}
