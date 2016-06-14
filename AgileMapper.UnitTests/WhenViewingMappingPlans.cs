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

            plan.ShouldContain("publicProperty_String.Value = omc.Source.Value;");
        }

        [Fact]
        public void ShouldIncludeAComplexTypeMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PersonViewModel>()
                .ToANew<Person>();

            plan.ShouldContain("person.Name = omc.Source.Name;");
            plan.ShouldContain("address.Line1 = omc.Source.AddressLine1;");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeEnumerableMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PublicProperty<int[]>>()
                .ToANew<PublicField<IEnumerable<int>>>();

            plan.ShouldContain("omc.Source.ForEach");
            plan.ShouldContain("int32s.Add");
        }

        [Fact]
        public void ShouldIncludeASimpleTypeMemberConversion()
        {
            var plan = Mapper
                .GetPlanFor<PublicProperty<Guid>>()
                .ToANew<PublicField<string>>();

            plan.ShouldContain("omc.Source.Value.ToString(");
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

                plan.ShouldContain("personViewModel.Name = (omc.Source.Title + \" \") + omc.Source.Name");
            }
        }

        [Fact]
        public void ShouldIncludeARootComplexTypeEnumerableMapping()
        {
            var plan = Mapper
                .GetPlanFor<IEnumerable<Person>>()
                .OnTo<IEnumerable<PersonViewModel>>();

            plan.ShouldContain("omc.Source.IntersectById");
            plan.ShouldContain("omc.Source.ExcludeById");
            plan.ShouldContain("personViewModels.Add");
        }

        [Fact]
        public void ShouldIncludeAComplexTypeEnumerableMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<IList<PersonViewModel>>()
                .Over<IEnumerable<Person>>();

            plan.ShouldContain("omc.Source.IntersectById");
            plan.ShouldContain("omc.Source.ExcludeById");

            plan.ShouldContain("IList<PersonViewModel> -> IEnumerable<Person>");
            plan.ShouldContain("PersonViewModel -> Person");
            plan.ShouldContain("PersonViewModel -> Address");
        }

        [Fact]
        public void ShouldNotDuplicateMappingPlans()
        {
            var plan = Mapper
                .GetPlanFor<IEnumerable<PersonViewModel>>()
                .OnTo<IEnumerable<Person>>();

            Regex.Matches(plan, "PersonViewModel -> Person").Cast<Match>().ShouldHaveSingleItem();
        }
    }
}
