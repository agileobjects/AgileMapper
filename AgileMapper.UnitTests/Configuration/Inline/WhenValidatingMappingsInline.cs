namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Validation;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenValidatingMappingsInline
    {
        [Fact]
        public void ShouldSupportMappingMemberValidation()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .Map(new PublicPropertyStruct<string>())
                    .OnTo(new PublicField<string>(), cfg => cfg
                        .ThrowNowIfMappingPlanIsIncomplete());
            }
        }

        [Fact]
        public void ShouldErrorIfMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper
                        .Map(new { Whatsit = "Thingy" })
                        .OnTo(new PublicSetMethod<string>(), cfg => cfg
                            .ThrowNowIfMappingPlanIsIncomplete()));

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicSetMethod<string>");
                validationEx.Message.ShouldContain("Rule set: Merge");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicSetMethod<string>.SetValue");
            }
        }

        [Fact]
        public void ShouldNotErrorIfMemberHasConfiguredDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new { Data = "Configure me!" })
                    .Over(new PublicField<string>(),
                        cfg => cfg.Map(ctx => ctx.Source.Data).To(pf => pf.Value),
                        cfg => cfg.ThrowNowIfMappingPlanIsIncomplete());

                result.Value.ShouldBe("Configure me!");
            }
        }

        [Fact]
        public void ShouldErrorIfMappingHasNonPairedEnumValues()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper
                        .Map(new PublicField<PaymentTypeUs>())
                        .ToANew<PublicField<PaymentTypeUk>>(cfg => cfg
                            .ThrowNowIfMappingPlanIsIncomplete()));

                validationEx.Message.ShouldContain("PublicField<PaymentTypeUs> -> PublicField<PaymentTypeUk>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Unpaired enum values");
                validationEx.Message.ShouldContain("PaymentTypeUs.Check matches no PaymentTypeUk");
                validationEx.Message.ShouldContain("PaymentTypeUk.Cheque is matched by no PaymentTypeUs");
            }
        }

        [Fact]
        public void ShouldNotErrorIfEnumValuesArePaired()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Should.NotThrow(() =>
                    mapper
                        .Map(new PublicField<PaymentTypeUs>())
                        .ToANew<PublicField<PaymentTypeUk>>(
                            cfg => cfg.WhenMapping.PairEnum(PaymentTypeUs.Check).With(PaymentTypeUk.Cheque),
                            cfg => cfg.ThrowNowIfMappingPlanIsIncomplete()));

            }
        }
    }
}
