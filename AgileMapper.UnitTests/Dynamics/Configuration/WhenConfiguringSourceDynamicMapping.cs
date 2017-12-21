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
                    .MapFullMemberName("LaLaLa")
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

                source.Value_0_StreetName = "Street Zero";
                source.Value_0_CityName = "City Zero";
                source.Value_1_StreetName = "Street One";
                source.Value_1_CityName = "City One";

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

                targetResult.Count.ShouldBe(3);

                targetResult["_0-Value"].ShouldBe(10);
                targetResult["_1-Value"].ShouldBe(20);
                targetResult["_2-Value"].ShouldBe(30);

                dynamic sourceDynamic = new ExpandoObject();

                ((IDictionary<string, object>)sourceDynamic)["Value+Value"] = 123;

                var sourceResult = (PublicField<PublicField<int>>)mapper.Map(sourceDynamic).ToANew<PublicField<PublicField<int>>>();

                sourceResult.Value.Value.ShouldBe(123);
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

                target.Value.ShouldBe(2);
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

                var personResult = (PersonViewModel)mapper.Map(source).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Person");

                source.Discount = 0.05;

                var customerResult = (PersonViewModel)mapper.Map(source).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                ((CustomerViewModel)customerResult).Discount.ShouldBe(0.05);

                source.Report = "Very good!";

                var mysteryCustomerResult = (PersonViewModel)mapper.Map(source).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Very good!");
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
