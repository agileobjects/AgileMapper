namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingFromDictionariesOnToEnumerableMembers
    {
        [Fact]
        public void ShouldMergeSimpleTypeListFromSimpleTypeDictionaryImplementationEntries()
        {
            IDictionary<string, string> source = new Dictionary<string, string>
            {
                ["Value[0]"] = "Zero",
                ["Value[1]"] = "One",
                ["Value[2]"] = "Two"
            };
            var target = new PublicField<List<string>>
            {
                Value = new List<string> { "One" }
            };
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe("One", "Zero", "Two");
        }

        [Fact]
        public void ShouldMergeSimpleTypeCollectionFromSimpleTypeDictionaryImplementationEntries()
        {
            var source = new StringKeyedDictionary<int>
            {
                ["Blah"] = 5,
                ["Value[0]"] = 20,
                ["Value[1]"] = 30
            };
            var target = new PublicField<ICollection<short?>>
            {
                Value = new List<short?> { 10, 20 }
            };
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotContain((short?)5);
            result.Value.ShouldBe((short?)10, (short?)20, (short?)30);
        }

        [Fact]
        public void ShouldMergeAComplexTypeArrayFromUntypedDictionaryImplementationEntries()
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