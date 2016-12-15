namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDictionaryMapping
    {
        [Fact]
        public void ShouldUseACustomFullDictionaryKeyForARootMember()
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

        [Fact]
        public void ShouldUseCustomMemberNameDictionaryKeysForRootMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries()
                    .Over<Address>()
                    .MapMemberName("HouseNumber")
                    .To(a => a.Line1)
                    .And
                    .MapMemberName("StreetName")
                    .To(a => a.Line2);

                var source = new Dictionary<string, string>
                {
                    ["HouseNumber"] = "123",
                    ["StreetName"] = "Street Road"
                };
                var target = new Address { Line1 = "Unknown", Line2 = "Unknown" };
                var result = mapper.Map(source).Over(target);

                result.Line1.ShouldBe("123");
                result.Line2.ShouldBe("Street Road");
            }
        }

        [Fact]
        public void ShouldUseACustomMemberNameDictionaryKeyForANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionaries()
                    .ToANew<Address>()
                    .MapMemberName("HouseName")
                    .To(a => a.Line1);

                var source = new Dictionary<string, string> { ["Value.HouseName"] = "Home" };
                var result = mapper.Map(source).ToANew<PublicProperty<Address>>();

                result.Value.Line1.ShouldBe("Home");
            }
        }
    }
}
