namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
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
                        .ThrowNowIfMappingIsIncomplete());
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
                            .ThrowNowIfMappingIsIncomplete()));

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
                        cfg => cfg.ThrowNowIfMappingIsIncomplete());

                result.Value.ShouldBe("Configure me!");
            }
        }
    }
}
