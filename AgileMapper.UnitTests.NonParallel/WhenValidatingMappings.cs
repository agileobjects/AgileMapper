namespace AgileObjects.AgileMapper.UnitTests.NonParallel
{
    using Common;
    using Common.TestClasses;
    using TestClasses;
    using Validation;
    using Xunit;

    public class WhenValidatingMappings : NonParallelTestsBase
    {
        [Fact]
        public void ShouldSupportCachedMappingMemberValidationViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                Should.NotThrow(Mapper.ThrowNowIfAnyMappingIsIncomplete);
            });
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<Customer>().ToANew<PublicField<long>>();

                var validationEx = Should.Throw<MappingValidationException>(
                    Mapper.ThrowNowIfAnyMappingIsIncomplete);

                validationEx.Message.ShouldContain("Customer -> PublicField<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicField<long>.Value");
            });
        }

        [Fact]
        public void ShouldValidateMappingPlanMemberMappingByDefaultViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    Mapper
                        .Map(new PublicField<int> { Value = 999 })
                        .ToANew<PublicTwoFields<long, long>>(cfg => cfg
                            .Map(ctx => ctx.Source.Value)
                            .To(ptf => ptf.Value2)));

                validationEx.Message.ShouldContain("PublicField<int> -> PublicTwoFields<long, long>");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicTwoFields<long, long>.Value1");
                validationEx.Message.ShouldNotContain("PublicTwoFields<long, long>.Value2");
            });
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedMembersHaveConfiguredDataSourcesViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.WhenMapping
                    .ThrowIfAnyMappingPlanIsIncomplete()
                    .AndWhenMapping
                    .From<PublicField<long>>()
                    .ToANew<PublicTwoFields<long, long>>()
                    .Map(ctx => ctx.Source.Value)
                    .To(ptf => ptf.Value1)
                    .And
                    .Map(ctx => ctx.Source.Value)
                    .To(ptf => ptf.Value2);

                Should.NotThrow(() =>
                    Mapper
                        .Map(new PublicField<long> { Value = 11 })
                        .ToANew<PublicTwoFields<long, long>>());
            });
        }
    }
}
