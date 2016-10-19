namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldIncludeASimpleTypeMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PublicField<string>>()
                .ToANew<PublicProperty<string>>();

            plan.ShouldContain("publicProperty_String.Value = data.Source.Value;");
        }

        [Fact]
        public void ShouldIncludeAComplexTypeMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .ToANew<Person>();

            plan.ShouldContain("person.Name = data.Source.Name;");
            plan.ShouldContain("address.Line1 = addressData.Source.AddressLine1;");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeEnumerableMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PublicProperty<int[]>>()
                .ToANew<PublicField<IEnumerable<int>>>();

            plan.ShouldContain("targetInt32s = ");
            plan.ShouldContain("new List<int>(data.Target)");
            plan.ShouldContain("targetInt32s.Add(sourceInt32s[i])");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeMemberConversion()
        {
            var plan = Mapper
                .GetPlanFor<PublicProperty<Guid>>()
                .ToANew<PublicField<string>>();

            plan.ShouldContain("data.Source.Value.ToString(");
        }

        [Fact]
        public void ShouldIncludeAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .From<Person>()
                    .Over<PersonViewModel>()
                    .Map((p, pvm) => p.Title + " " + p.Name)
                    .To(pvm => pvm.Name);

                var plan = mapper
                    .GetPlanFor<Person>()
                    .Over<PersonViewModel>();

                plan.ShouldContain("personViewModel.Name = data.Source.Title + \" \" + data.Source.Name");
            }
        }

        [Fact]
        public void ShouldIncludeARootComplexTypeEnumerableMapping()
        {
            var plan = Mapper
                .GetPlanFor<IEnumerable<Person>>()
                .OnTo<IEnumerable<PersonViewModel>>();

            plan.ShouldContain("collectionData.Intersection.ForEach(data.Map)");
            plan.ShouldContain("persons = collectionData.NewSourceItems");
        }

        [Fact]
        public void ShouldIncludeAComplexTypeEnumerableMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<IList<PersonViewModel>>()
                .Over<IEnumerable<Person>>();

            plan.ShouldContain("personViewModels = collectionData.NewSourceItems");
            plan.ShouldContain("collectionData.Intersection.ForEach(data.Map)");
            plan.ShouldContain("collectionData.AbsentTargetItems.ForEach(persons.Remove)");

            plan.ShouldContain("IList<PersonViewModel> -> IEnumerable<Person>");
            plan.ShouldContain("PersonViewModel -> Person");
            plan.ShouldNotContain("PersonViewModel -> Address"); // <- because the Address mapping is inlined
        }

        [Fact]
        public void ShouldNotDuplicateMappingPlans()
        {
            var plan = Mapper
                .GetPlanFor<IEnumerable<PersonViewModel>>()
                .OnTo<IEnumerable<Person>>();

            Regex.Matches(plan, "PersonViewModel -> Person").Cast<Match>().ShouldHaveSingleItem();
        }

        [Fact]
        public void ShouldIncludeAMemberWithNoDataSource()
        {
            var plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .OnTo<Person>();

            plan.ShouldContain("// No data source for Title");
        }

        [Fact]
        public void ShouldIncludeAnIgnoredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .To<PersonViewModel>()
                    .Ignore(pvm => pvm.AddressLine1);

                var plan = mapper
                    .GetPlanFor<Person>()
                    .ToANew<PersonViewModel>();

                plan.ShouldContain("// AddressLine1 is ignored");
            }
        }

        [Fact]
        public void ShouldNotIncludeASourceMemberWithTheSameConditionAsAConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .From<PublicField<PublicField<string>>>()
                    .ToANew<PublicProperty<string>>()
                    .Map((pf, pp) => pf.Value.Value)
                    .To(pp => pp.Value);

                var plan = mapper
                    .GetPlanFor<PublicField<PublicField<string>>>()
                    .ToANew<PublicProperty<string>>();

                plan.ShouldContain("data.Source.Value.Value");
                plan.ShouldNotContain("data.Source.Value.ToString()");
            }
        }

        [Fact]
        public void ShouldUseNestedInlineMappers()
        {
            var plan = Mapper
                .GetPlanFor<PublicField<PublicField<PublicField<int>>>>()
                .ToANew<PublicField<PublicSealed<PublicSealed<long>>>>();

            var numberOfHeaders = Regex.Matches(plan, "// Map ").Count;

            numberOfHeaders.ShouldBe(1);
        }
    }
}
