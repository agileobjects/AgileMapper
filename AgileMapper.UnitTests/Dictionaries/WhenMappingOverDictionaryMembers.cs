namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverDictionaryMembers
    {
        [Fact]
        public void ShouldOverwriteANestedSimpleTypedIDictionary()
        {
            var source = new PublicField<Address>
            {
                Value = new Address { Line1 = "Here", Line2 = "There" }
            };
            var target = new PublicProperty<IDictionary<string, string>>
            {
                Value = new Dictionary<string, string> { ["Line1"] = "La la la" }
            };
            var result = Mapper.Map(source).Over(target);

            result.Value["Line1"].ShouldBe("Here");
            result.Value["Line2"].ShouldBe("There");
        }

        [Fact]
        public void ShouldOverwriteAComplexTypeArrayToANestedSameComplexTypeDictionary()
        {
            var source = new PublicField<Address[]>
            {
                Value = new[]
                {
                    new Address { Line1 = "1.1", Line2 = null },
                    new Address { Line1 = "2.1", Line2 = "2.2" }
                }
            };
            var target = new PublicReadOnlyField<Dictionary<string, Address>>(
                new Dictionary<string, Address>
                {
                    ["[0]"] = new Address { Line1 = "Old 1.1", Line2 = null },
                    ["[1]"] = default(Address)
                });
            var existingAddress = target.Value["[0]"];
            var result = Mapper.Map(source).Over(target);

            result.Value["[0]"].ShouldBeSameAs(existingAddress);
            result.Value["[0]"].Line1.ShouldBe("1.1");
            result.Value["[0]"].Line2.ShouldBeNull();
            result.Value["[1]"].ShouldNotBeNull();
            result.Value["[1]"].Line1.ShouldBe("2.1");
            result.Value["[1]"].Line2.ShouldBe("2.2");
        }

        [Fact]
        public void ShouldOverwriteADictionaryToAConvertibleSimpleTypedDictionary()
        {
            var source = new PublicProperty<IDictionary<string, int>>
            {
                Value = new Dictionary<string, int> { ["One"] = 1, ["Three"] = 3, ["Five"] = 5 }
            };
            var target = new PublicField<IDictionary<string, short?>>
            {
                Value = new Dictionary<string, short?> { ["Two"] = 2, ["Three"] = 7578, ["Four"] = null }
            };
            Mapper.Map(source).Over(target);

            target.Value["One"].ShouldBe(1);
            target.Value["Two"].ShouldBe(2);
            target.Value["Three"].ShouldBe(3);
            target.Value["Four"].ShouldBeNull();
            target.Value["Five"].ShouldBe(5);
        }

        // See https://github.com/agileobjects/AgileMapper/issues/10
        [Fact]
        public void ShouldMapADictionaryMemberOverADictionaryMember()
        {
            var source = new PublicProperty<Dictionary<string, object>>
            {
                Value = new Dictionary<string, object>
                {
                    ["One!"] = new PersonViewModel { Name = "One!" },
                    ["Two!"] = new PersonViewModel { Name = "Two!" }
                }
            };


            var target = new PublicProperty<Dictionary<string, object>>
            {
                Value = new Dictionary<string, object>
                {
                    ["Two!"] = new PersonViewModel { Name = "Three!" }
                }
            };

            var existingTarget = target.Value;

            Mapper.Map(source).Over(target);

            target.Value.ShouldBeSameAs(existingTarget);

            target.Value.ContainsKey("One!").ShouldBeTrue();
            target.Value["One!"].ShouldBeOfType<PersonViewModel>();
            ((PersonViewModel)target.Value["One!"]).Name.ShouldBe("One!");

            target.Value.ContainsKey("Two!").ShouldBeTrue();
            target.Value["Two!"].ShouldBeOfType<PersonViewModel>();
            ((PersonViewModel)target.Value["Two!"]).Name.ShouldBe("Two!");
        }
    }
}