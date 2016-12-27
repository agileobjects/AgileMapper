namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionaryMembers
    {
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
    }
}
