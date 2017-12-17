namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
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
        public void ShouldApplyACustomMemberNamePartsToASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .OnTo<Address>()
                    .MapMemberName("StreetName")
                    .To(a => a.Line1)
                    .And
                    .MapMemberName("CityName")
                    .To(a => a.Line2);

                dynamic source = new ExpandoObject();

                source.Value_0__StreetName = "Street Zero";
                source.Value_0__CityName = "City Zero";
                source.Value_1__StreetName = "Street One";
                source.Value_1__CityName = "City One";

                var target = new PublicField<IList<Address>> { Value = new List<Address>() };

                mapper.Map(source).OnTo(target);

                target.Value.Count.ShouldBe(2);

                target.Value.First().Line1.ShouldBe("Street Zero");
                target.Value.First().Line2.ShouldBe("City Zero");

                target.Value.Second().Line1.ShouldBe("Street One");
                target.Value.Second().Line2.ShouldBe("City One");
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

        [Fact]
        public void ShouldNotConflictDynamicAndDictionaryConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<object>()
                    .UseMemberNameSeparator("-");

                mapper.WhenMapping
                    .FromDynamics
                    .UseMemberNameSeparator("+");

                var dictionarySource = new Dictionary<string, object>
                {
                    ["Value-Line1"] = "Line 1!",
                    ["Value-Line2"] = "Line 2!"
                };

                var dictionaryResult = mapper.Map(dictionarySource).ToANew<PublicField<Address>>();

                dictionaryResult.Value.ShouldNotBeNull();
                dictionaryResult.Value.Line1.ShouldBe("Line 1!");
                dictionaryResult.Value.Line2.ShouldBe("Line 2!");

                dynamic dynamicSource = new ExpandoObject();

                ((IDictionary<string, object>)dynamicSource)["Value+Line1"] = "Line 1?!";
                ((IDictionary<string, object>)dynamicSource)["Value+Line2"] = "Line 2?!";

                var dynamicResult = (PublicField<Address>)mapper.Map(dynamicSource).ToANew<PublicField<Address>>();

                dynamicResult.Value.ShouldNotBeNull();
                dynamicResult.Value.Line1.ShouldBe("Line 1?!");
                dynamicResult.Value.Line2.ShouldBe("Line 2?!");
            }
        }
    }
}
