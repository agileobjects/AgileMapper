namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringNestedDictionaryMapping
    {
        [Fact]
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
    }
}
