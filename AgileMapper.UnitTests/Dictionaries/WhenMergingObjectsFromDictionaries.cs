namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMergingObjectsFromDictionaries
    {
        [Fact]
        public void ShouldPopulateAStringMemberFromANullableTypedEntry()
        {
            var guid = Guid.NewGuid();

            var source = new Dictionary<string, Guid?> { ["Value"] = guid };
            var target = new PublicProperty<string>();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe(guid.ToString());
        }

        [Fact]
        public void ShouldMergeANestedComplexTypeArrayFromUntypedDictionaryImplementationEntries()
        {
            var source = new StringKeyedDictionary<object>
            {
                ["Value[0]"] = new CustomerViewModel { Name = "Mr Pants" },
                ["Value[1]"] = new PersonViewModel { Name = "Ms Blouse" }
            };
            var target = new PublicProperty<PersonViewModel[]>
            {
                Value = new PersonViewModel[]
                {
                    new CustomerViewModel { Name = "Sir Trousers", Discount = 0.99 }
                }
            };
            var originalValue1 = target.Value.First();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.Length.ShouldBe(3);

            result.Value.First().ShouldBe(originalValue1);
            result.Value.Second().Name.ShouldBe("Mr Pants");
            result.Value.Third().Name.ShouldBe("Ms Blouse");
        }

        [Fact]
        public void ShouldReuseAnExistingListIfNoEntriesMatch()
        {
            var source = new Dictionary<string, object>();
            var target = new PublicProperty<ICollection<string>> { Value = new List<string>() };
            var originalList = target.Value;
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(originalList);
        }
    }
}