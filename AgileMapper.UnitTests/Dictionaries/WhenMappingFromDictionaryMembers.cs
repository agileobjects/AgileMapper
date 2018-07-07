namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingFromDictionaryMembers
    {
        [Fact]
        public void ShouldPopulateANestedStringFromANestedObjectEntry()
        {
            var source = new PublicField<Dictionary<string, object>>
            {
                Value = new Dictionary<string, object>
                {
                    ["Line1"] = "6478 Nested Drive"
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Address>>();

            result.Value.ShouldNotBeNull();
            result.Value.Line1.ShouldBe("6478 Nested Drive");
        }

        [Fact]
        public void ShouldPopulateANestedIntArrayFromNestedConvertibleTypedEntries()
        {
            var source = new PublicField<Dictionary<string, short>>
            {
                Value = new Dictionary<string, short>
                {
                    ["[0]"] = 6478,
                    ["[1]"] = 9832,
                    ["[2]"] = 1028
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<int[]>>();

            result.Value.ShouldNotBeNull();
            result.Value.Length.ShouldBe(3);
            result.Value.ShouldBe(6478, 9832, 1028);
        }

        [Fact]
        public void ShouldPopulateANestedGuidEnumerableFromNestedConvertibleUntypedEntries()
        {
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();

            var source = new PublicField<Dictionary<string, object>>
            {
                Value = new Dictionary<string, object>
                {
                    ["[0]"] = guidOne,
                    ["[1]"] = guidTwo
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<Guid>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe(guidOne, guidTwo);
        }

        [Fact]
        public void ShouldPopulateANestedComplexTypeListFromNestedStringEntries()
        {
            var source = new PublicField<Dictionary<string, string>>
            {
                Value = new Dictionary<string, string>
                {
                    ["[0].Value"] = "Item 1!",
                    ["[1].Value"] = "Item 2!"
                }
            };
            var result = Mapper.Map(source).ToANew<PublicField<List<PublicProperty<string>>>>();

            result.Value.Count.ShouldBe(2);
            result.Value.ShouldBe(pp => pp.Value, "Item 1!", "Item 2!");
        }
    }
}
