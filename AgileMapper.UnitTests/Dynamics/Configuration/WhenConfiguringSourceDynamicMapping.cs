namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringSourceDynamicMapping
    {
        [Fact]
        public void ShouldUseACustomDynamicSourceMemberName()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .ToANew<PublicField<int>>()
                    .MapMember("LaLaLa")
                    .To(pf => pf.Value);

                dynamic source = new ExpandoObject();

                source.LaLaLa = 1;
                source.Value = 2;

                var result = (PublicField<int>)mapper.Map(source).ToANew<PublicField<int>>();

                result.ShouldNotBeNull();
                result.Value.ShouldBe(1);

                mapper.Map(source).Over(result);

                result.Value.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldUseCustomDynamicMemberNameForRootMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .Over<Address>()
                    .MapMemberName("HouseNumber")
                    .To(a => a.Line1)
                    .And
                    .MapMemberName("Street")
                    .To(a => a.Line2);

                dynamic source = new ExpandoObject();

                source.HouseNumber = 10;
                source.Street = "Street Road";

                var target = new Address { Line1 = "??", Line2 = "??" };

                mapper.Map(source).Over(target);

                target.Line1.ShouldBe("10");
                target.Line2.ShouldBe("Street Road");
            }
        }

        [Fact]
        public void ShouldNotApplyDictionaryConfigurationToDynamics()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<object>()
                    .To<PublicField<string>>()
                    .MapFullKey("LaLaLa")
                    .To(pf => pf.Value);

                dynamic source = new ExpandoObject();

                source.LaLaLa = 1;
                source.Value = 2;

                var result = (PublicField<string>)mapper.Map(source).ToANew<PublicField<string>>();

                result.ShouldNotBeNull();
                result.Value.ShouldBe("2");
            }
        }

        [Fact]
        public void ShouldNotApplyDynamicConfigurationToDictionaries()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .To<PublicField<string>>()
                    .MapMember("LaLaLa")
                    .To(pf => pf.Value);

                var source = new Dictionary<string, int>
                {
                    ["LaLaLa"] = 1,
                    ["Value"] = 2
                };

                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.ShouldNotBeNull();
                result.Value.ShouldBe("2");
            }
        }
    }
}
