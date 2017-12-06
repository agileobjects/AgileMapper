namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionariesOnToComplexTypes
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
    }
}