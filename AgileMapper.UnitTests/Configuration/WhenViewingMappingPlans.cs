namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.ObjectModel;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
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

                string plan = mapper
                    .GetPlanFor<Person>()
                    .Over<PersonViewModel>();

                plan.ShouldContain("pToPvmData.Target.Name = sourcePerson.Title + \" \" + sourcePerson.Name");
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

                string plan = mapper
                    .GetPlanFor<PublicField<PublicField<string>>>()
                    .ToANew<PublicProperty<string>>();

                plan.ShouldContain("data.Source.Value.Value");
                plan.ShouldNotContain("data.Source.Value.ToString()");
            }
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

                string plan = mapper
                    .GetPlanFor<Person>()
                    .ToANew<PersonViewModel>();

                plan.ShouldContain("// AddressLine1 is ignored");
            }
        }

        [Fact]
        public void ShouldIncludeMemberFilterExpressions()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Address>()
                    .IgnoreTargetMembersWhere(member => member.IsPropertyMatching(p => p.Name == "Line2"));

                string plan = mapper.GetPlanFor<Address>().ToANew<Address>();

                plan.ShouldContain("member.IsPropertyMatching(p => p.Name == \"Line2\")");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/13
        [Fact]
        public void ShouldShowMapChildObjectCalls()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicTwoFields<string, object>>()
                    .Map((pp, ptf) => pp.Value)
                    .To(ptf => ptf.Value1);


                string plan = mapper
                    .GetPlanFor<PublicProperty<string>>()
                    .ToANew<PublicTwoFields<string, object>>();

                plan.ShouldContain("// Map PublicProperty<string> -> PublicTwoFields<string, object>");
                plan.ShouldContain(".Value1 = ppsToPtfsoData.Source.Value");
                plan.ShouldContain("// No data source for Value2");
            }
        }

        [Fact]
        public void ShouldShowMultipleEnumSourceMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .To<OrderUs>()
                    .Map((s, o) => s.Value1).To(o => o.PaymentType)
                    .But
                    .If((s, o) => s.Value1 == PaymentTypeUk.Cheque)
                    .Map((s, o) => s.Value2).To(o => o.PaymentType);

                string plan = mapper
                    .GetPlanFor<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .ToANew<OrderUs>();

                plan.ShouldContain("PublicTwoFields<PaymentTypeUk, PaymentTypeUs>.Value1 to OrderUs.PaymentType");
                plan.ShouldContain("Value1 == PaymentTypeUk.Cheque");
                plan.ShouldContain("PaymentTypeUk.Cheque matches no PaymentTypeUs");
                plan.ShouldContain("PaymentTypeUs.Check is matched by no PaymentTypeUk");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyArrayMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string[]>>()
                    .To<PublicReadOnlyField<int[]>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor<int[]>();

                string plan = mapper
                    .GetPlanFor<PublicField<string[]>>()
                    .ToANew<PublicReadOnlyField<int[]>>();

                plan.ShouldContain("readonly array");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyReadOnlyCollectionMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string[]>>()
                    .To<PublicReadOnlyField<ReadOnlyCollection<int>>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor("readOnlyValue");

                string plan = mapper
                    .GetPlanFor<PublicField<string[]>>()
                    .ToANew<PublicReadOnlyField<ReadOnlyCollection<int>>>();

                plan.ShouldContain("readonly ReadOnlyCollection<int>");
            }
        }

        [Fact]
        public void ShouldIncludeUnmappableReadOnlyIntMemberDetails()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicReadOnlyField<int>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToCtor("readOnlyValue");

                string plan = mapper
                    .GetPlanFor<PublicField<string>>()
                    .ToANew<PublicReadOnlyField<int>>();

                plan.ShouldContain("readonly int");
            }
        }
    }
}
