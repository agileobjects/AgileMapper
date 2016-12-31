namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

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
            result["Address.Line1"].ShouldBe("Customer house");
            result["Address.Line2"].ShouldBeNull();
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
        public void ShouldMapBetweenSameSimpleValueTypedDictionaries()
        {
            var source = new Dictionary<string, int> { ["One"] = 1, ["Two"] = 2 };
            var result = Mapper.Map(source).ToANew<Dictionary<string, int>>();

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

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);
            result["1"].ShouldBe(1);
            result["2"].ShouldBe(2);
        }

        [Fact]
        public void ShouldMapBetweenDifferentSimpleValueTypeDictionaries()
        {
            var source = new Dictionary<string, char> { ["One"] = '1', ["Two"] = '2' };
            var result = Mapper.Map(source).ToANew<Dictionary<string, long>>();

            result.ShouldNotBeSameAs(source);
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

            result.ShouldNotBeSameAs(source);
            result.Count.ShouldBe(2);

            result["123"].Name.ShouldBe("Bobby");
            result["123"].Address.Line1.ShouldBe("Address 1");

            result["456"].Name.ShouldBe("Magnus");
            result["456"].Address.Line1.ShouldBe("Address 2!");

        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            var source = new MysteryCustomer { Name = "Richie", Address = null };
            var result = Mapper.Map(source).ToANew<Dictionary<string, string>>();

            result["Name"].ShouldBe("Richie");
            result.ContainsKey("Address.Line1").ShouldBeFalse();
            result.ContainsKey("Address.Line2").ShouldBeFalse();
        }

        [Fact]
        public void ShouldHandleAnInvalidKeyTypeTargetDictionary()
        {
            var source = new PublicField<string> { Value = "DateTime keys?!" };
            var result = Mapper.Map(source).ToANew<Dictionary<DateTime, string>>();

            result.ShouldBeEmpty();
        }
    }
}
