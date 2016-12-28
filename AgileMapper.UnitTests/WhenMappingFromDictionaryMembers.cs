namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

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

        //[Fact]
        public void ShouldPopulateANestedStringFromAConfiguredNestedObjectEntry()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Dictionary<string, object>>>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Value)
                    .To(p => p.Address);

                var source = new PublicField<Dictionary<string, object>>
                {
                    Value = new Dictionary<string, object>
                    {
                        ["Line1"] = "6478 Nested Drive"
                    }
                };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("6478 Nested Drive");
            }
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
    }
}
