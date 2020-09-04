#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Common;
    using Common.TestClasses;
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
                    .MapFullMemberName("LaLaLa")
                    .To(pf => pf.Value);

                dynamic source = new ExpandoObject();

                source.LaLaLa = 1;
                source.Value = 2;

                var result = mapper.Map(source).ToANew<PublicField<int>>();

                Assert.NotNull(result);
                Assert.Equal(1, result.Value);

                mapper.Map(source).Over(result);
                
                Assert.Equal(2, result.Value);
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
                    .UseMemberNameSeparator("-")
                    .And
                    .MapMemberName("HouseNumber")
                    .To(a => a.Line1)
                    .And
                    .MapMemberName("Street")
                    .To(a => a.Line2);

                dynamic source = new ExpandoObject();

                ((IDictionary<string, object>)source)["Value-HouseNumber"] = 10;
                ((IDictionary<string, object>)source)["Value-Street"] = "Street Road";

                var target = new PublicField<Address>
                {
                    Value = new Address { Line1 = "??", Line2 = "??" }
                };

                mapper.Map(source).Over(target);

                Assert.Equal("10", target.Value.Line1);
                Assert.Equal("Street Road", target.Value.Line2);
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

                source.Value_0_StreetName = "Street Zero";
                source.Value_0_CityName = "City Zero";
                source.Value_1_StreetName = "Street One";
                source.Value_1_CityName = "City One";

                var target = new PublicField<IList<Address>> { Value = new List<Address>() };

                mapper.Map(source).OnTo(target);
                
                Assert.Equal(2, target.Value.Count);
                
                Assert.Equal("Street Zero", target.Value[0].Line1);
                Assert.Equal("City Zero", target.Value[0].Line2);
                
                Assert.Equal("Street One", target.Value[1].Line1);
                Assert.Equal("City One", target.Value[1].Line2);
            }
        }

        [Fact]
        public void ShouldApplyCustomSeparators()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dynamics
                    .UseMemberNameSeparator("-")
                    .AndWhenMapping
                    .FromDynamics
                    .UseMemberNameSeparator("+");

                var source = new[]
                {
                    new PublicProperty<int> { Value = 10 },
                    new PublicProperty<int> { Value = 20 },
                    new PublicProperty<int> { Value = 30 },
                };

                dynamic targetDynamic = new ExpandoObject();

                ((IDictionary<string, object>)targetDynamic)["_0-Value"] = 1;
                ((IDictionary<string, object>)targetDynamic)["_1-Value"] = 2;

                var targetResult = (IDictionary<string, object>)mapper.Map(source).Over(targetDynamic);
                
                Assert.Equal(3, targetResult.Count);

                Assert.Equal(10, targetResult["_0-Value"]);
                Assert.Equal(20, targetResult["_1-Value"]);
                Assert.Equal(30, targetResult["_2-Value"]);

                dynamic sourceDynamic = new ExpandoObject();

                ((IDictionary<string, object>)sourceDynamic)["Value+Value"] = 123;

                var sourceResult = mapper.Map(sourceDynamic).ToANew<PublicField<PublicField<int>>>();
                
                Assert.Equal(123, sourceResult.Value.Value);
            }
        }

        [Fact]
        public void ShouldApplyACustomConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .Over<PublicField<long>>()
                    .Map((d, pf) => d.Count)
                    .To(pf => pf.Value);

                dynamic source = new ExpandoObject();
                source.One = 1;
                source.Two = 2;

                var target = new PublicField<long>();

                mapper.Map(source).Over(target);

                Assert.Equal(2, target.Value);
            }
        }

        [Fact]
        public void ShouldConditionallyMapToDerivedTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .ToANew<PersonViewModel>()
                    .If(s => s.Source.ContainsKey("Discount"))
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(s => s.Source.ContainsKey("Report"))
                    .MapTo<MysteryCustomerViewModel>();

                dynamic source = new ExpandoObject();

                source.Name = "Person";

                var personResult = mapper.Map(source).ToANew<PersonViewModel>();

                Assert.IsType<PersonViewModel>(personResult);
                Assert.Equal("Person", personResult.Name);

                source.Discount = 0.05;

                var customerResult = mapper.Map(source).ToANew<PersonViewModel>();
                
                Assert.IsType<CustomerViewModel>(customerResult);
                Assert.Equal(0.05, ((CustomerViewModel)customerResult).Discount);

                source.Report = "Very good!";

                var mysteryCustomerResult = mapper.Map(source).ToANew<PersonViewModel>();
                
                Assert.IsType<MysteryCustomerViewModel>(mysteryCustomerResult);
                Assert.Equal("Very good!", ((MysteryCustomerViewModel)mysteryCustomerResult).Report);
            }
        }

        [Fact]
        public void ShouldMapToASimpleTypeArrayWithACustomElementSeparator()
        {
            dynamic source = new ExpandoObject();

            ((IDictionary<string, object>)source)["-0-"] = "a";
            ((IDictionary<string, object>)source)["-1-"] = "b";
            ((IDictionary<string, object>)source)["-2-"] = "c";

            var result = Mapper.Map(source).ToANew<char[]>(
                new Expression<Action<IFullMappingInlineConfigurator<ExpandoObject, char[]>>>[]
                {
                    cfg => cfg.WhenMapping.FromDynamics.UseElementKeyPattern("-i-")
                });

            Assert.Equal(3, result.Length);
            Assert.Equal('a', result[0]);
            Assert.Equal('b', result[1]);
            Assert.Equal('c', result[2]);
        }

        [Fact]
        public void ShouldMapToANestedSimpleTypeListWithACustomElementSeparator()
        {
            dynamic source = new ExpandoObject();

            ((IDictionary<string, object>)source)["Value[0]"] = "abc";
            ((IDictionary<string, object>)source)["Value[1]"] = "xyz";
            ((IDictionary<string, object>)source)["Value[2]"] = "123";

            var result = Mapper.Map(source).ToANew<PublicField<List<string>>>(
                new Expression<Action<IFullMappingInlineConfigurator<ExpandoObject, PublicField<List<string>>>>>[]
                {
                    cfg => cfg
                        .WhenMapping
                        .FromDynamics
                        .ToANew<List<string>>()
                        .UseElementKeyPattern("[i]")
                });

            Assert.Equal(3, result.Value.Count);
            Assert.Equal("abc", result.Value[0]);
            Assert.Equal("xyz", result.Value[1]);
            Assert.Equal("123", result.Value[2]);
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

                var result = mapper.Map(source).ToANew<PublicField<string>>();

                Assert.NotNull(result);
                Assert.Equal("2", result.Value);
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
                    .MapFullMemberName("LaLaLa")
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

                Assert.NotNull(dictionaryResult.Value);
                Assert.Equal("Line 1!", dictionaryResult.Value.Line1);
                Assert.Equal("Line 2!", dictionaryResult.Value.Line2);

                dynamic dynamicSource = new ExpandoObject();

                ((IDictionary<string, object>)dynamicSource)["Value+Line1"] = "Line 1?!";
                ((IDictionary<string, object>)dynamicSource)["Value+Line2"] = "Line 2?!";

                var dynamicResult = mapper.Map(dynamicSource).ToANew<PublicField<Address>>();

                Assert.NotNull(dynamicResult.Value);
                Assert.Equal("Line 1?!", dynamicResult.Value.Line1);
                Assert.Equal("Line 2?!", dynamicResult.Value.Line2);
            }
        }
    }
}
#endif