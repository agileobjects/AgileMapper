namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
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