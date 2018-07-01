namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenApplyingMapperConfigurations
    {
        [Fact]
        public void ShouldApplyAMapperConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseConfiguration.From<PfiToPfsMapperConfiguration>();

                PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
            }
        }

        [Fact]
        public void ShouldApplyMapperConfigurationsInAGivenTypeAssembly()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseConfiguration.FromAssemblyOf<WhenApplyingMapperConfigurations>();

                PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
            }
        }

        #region Helper Classes

        private class PfiToPfsMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(t => t.Value);

                GetPlanFor<PublicField<int>>().ToANew<PublicField<string>>();
            }

            public static void VerifyConfigured(IMapper mapper)
            {
                var source = new PublicField<int> { Value = 123 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(246);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private class PfsToPfiMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value + "10")
                    .To(t => t.Value);

                GetPlansFor<PublicField<string>>().To<PublicField<int>>();
            }

            public static void VerifyConfigured(IMapper mapper)
            {
                var source = new PublicField<string> { Value = "10" };
                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(1010);
            }
        }

        #endregion
    }
}
