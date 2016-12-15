namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDictionaryMapping
    {
        [Fact]
        public void ShouldUseACustomDictionaryKey()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries()
                    .To<PublicField<string>>()
                    .MapKey("BoomDiddyBoom")
                    .To(pf => pf.Value);

                var source = new Dictionary<string, int> { ["BoomDiddyBoom"] = 123 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("123");
            }
        }
    }
}
