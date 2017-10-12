namespace AgileObjects.AgileMapper.UnitTests.Structs.Dictionaries
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionariesToStructs
    {
        [Fact]
        public void ShouldPopulateAnIntMemberFromATypedEntry()
        {
            var source = new Dictionary<string, int> { ["Value"] = 123 };
            var result = Mapper.Map(source).ToANew<PublicCtorStruct<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldPopulateAStringMemberFromAnUntypedEntry()
        {
            var source = new Dictionary<string, object> { ["value"] = "Yes!" };
            var target = new PublicPropertyStruct<string> { Value = "No!" };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe("Yes!");
        }

        [Fact]
        public void ShouldPopulateANestedBoolMemberFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object> { ["value.value"] = "True" };
            var result = Mapper.Map(source).ToANew<PublicProperty<PublicPropertyStruct<bool>>>();

            result.Value.Value.ShouldBeTrue();
        }

        [Fact]
        public void ShouldPopulateARootReadOnlyCollectionFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].Value1"] = "1",
                ["[0].Value2"] = "2",
                ["[1].Value1"] = "3",
                ["[1].Value2"] = "4",
                ["[2].Value1"] = "5",
                ["[2].Value2"] = "6"
            };
            var result = Mapper.Map(source).ToANew<ReadOnlyCollection<PublicTwoFieldsStruct<int, int>>>();

            result.Count.ShouldBe(3);

            result.First().Value1.ShouldBe(1);
            result.First().Value2.ShouldBe(2);

            result.Second().Value1.ShouldBe(3);
            result.Second().Value2.ShouldBe(4);

            result.Third().Value1.ShouldBe(5);
            result.Third().Value2.ShouldBe(6);
        }

        [Fact]
        public void ShouldPopulateANestedCollectionFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["Value[0].Value"] = "NO WAY",
                ["Value[1].Value"] = "YES WAY"
            };
            var result = Mapper.Map(source).ToANew<PublicField<Collection<PublicCtorStruct<string>>>>();

            result.Value.Count.ShouldBe(2);
            result.Value.First().Value.ShouldBe("NO WAY");
            result.Value.Second().Value.ShouldBe("YES WAY");
        }
    }
}
