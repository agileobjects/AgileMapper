namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using TestClasses;
    using Validation;
    using Xunit;

    public abstract class WhenValidatingProjections<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenValidatingProjections(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldSupportCachedProjectionValidation()
        {
            return RunTest((context, mapper) =>
            {
                mapper.GetPlanForProjecting(context.Addresses).To<AddressDto>();

                Should.NotThrow(mapper.ThrowNowIfAnyMappingPlanIsIncomplete);

                return Task.FromResult(1);
            });
        }

        [Fact]
        public Task ShouldErrorIfCachedProjectionMembersHaveNoDataSources()
        {
            return RunTest((context, mapper) =>
            {
                mapper.GetPlanForProjecting(context.RotaEntries).To<RotaEntryDto>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("IQueryable<RotaEntry> -> IQueryable<RotaEntryDto>");
                validationEx.Message.ShouldContain("Rule set: Project");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("IQueryable<RotaEntryDto>[i].StartTime");
                validationEx.Message.ShouldContain("IQueryable<RotaEntryDto>[i].EndTime");

                return Task.FromResult(1);
            });
        }

        [Fact]
        public Task ShouldErrorIfCachedProjectionTargetTypeIsUnconstructable()
        {
            return RunTest((context, mapper) =>
            {
                mapper.GetPlanForProjecting(context.Addresses).To<PublicStringCtorDto>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("IQueryable<Address> -> IQueryable<PublicStringCtorDto>");
                validationEx.Message.ShouldContain("Rule set: Project");
                validationEx.Message.ShouldContain("Unmappable target Types");
                validationEx.Message.ShouldContain("Address -> PublicStringCtorDto");

                return Task.FromResult(1);
            });
        }
    }
}
