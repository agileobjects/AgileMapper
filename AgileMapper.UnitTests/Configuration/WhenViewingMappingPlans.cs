namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using AgileMapper.Members;
    using Common;
    using Common.TestClasses;
    using MoreTestClasses.Vb;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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

                plan.ShouldContain("ata.Source.Value.Value");
                plan.ShouldNotContain("ata.Source.Value.ToString()");
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
                plan.ShouldContain(".Value1 = sppToSoptfData.Source.Value");
                plan.ShouldContain("// No data sources for Value2");
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
        public void ShouldIncludeUnmappableIndexedPropertyDetails()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFields<PublicField<string>, PublicField<int>>>()
                .ToANew<PublicNamedIndex<PublicField<string>, PublicField<int>>>();

            plan.ShouldContain("requires index(es) - indexOne: int, indexTwo: int");
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

        // See https://github.com/agileobjects/AgileMapper/issues/168
        [Fact]
        public void ShouldIncludeADataSourceFuncInvocation()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<IMappingData<Issue168.ISomething, Issue168.TgtSomething>, Issue168.SrcEnum> getEnum =
                    ctxt => ctxt.Source is Issue168.SomethingB someB ? someB.MyLargeEnum : Issue168.SrcEnum.A;

                mapper.WhenMapping
                    .From<Issue168.ISomething>().To<Issue168.TgtSomething>()
                    .Map(getEnum).To(tgt => tgt.MyLargeEnumg);

                string mappingPlan = mapper.GetPlanFor<Issue168.ISomething>().ToANew<Issue168.TgtSomething>();

                mappingPlan.ShouldNotBeNull();
                mappingPlan.ShouldContain("srcEnumResult = ");
                mappingPlan.ShouldContain(".Invoke(sToTsData)");
            }
        }

        #region Helper Members

        internal static class Issue168
        {
            public enum SrcEnum
            {
                A, B, C, D, E, F, G, H, I
            }

            public enum TgtEnum
            {
                A, B, C, D, E, F, G, H, I
            }

            public interface ISomething
            {
                string Name { get; }
            }

            public class SomethingB : ISomething
            {
                public string Name { get; set; }

                public SrcEnum MyLargeEnum { get; set; }
            }
            public class TgtSomething
            {
                public string Name { get; set; }

                public TgtEnum MyLargeEnumg { get; set; }
            }
        }

        #endregion
    }
}
