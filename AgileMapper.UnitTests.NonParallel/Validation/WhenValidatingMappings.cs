namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Validation
{
    using TestClasses;
    using Xunit;
    using Shouldly;

    public class WhenValidatingMappings : NonParallelTestsBase
    {
        [Fact]
        public void ShouldSupportCachedMappingMemberValidationFromTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                Should.NotThrow(() => Mapper.ThrowNowIfAnyMappingIsIncomplete());
            });
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            TestThenReset(() =>
            {
                Mapper.GetPlanFor<Customer>().ToANew<PublicField<long>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    Mapper.ThrowNowIfAnyMappingIsIncomplete());

                validationEx.Message.ShouldContain("Customer -> PublicField<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicField<long>.Value");
            });
        }
    }
}
