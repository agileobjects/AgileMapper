namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using MoreTestClasses;
    using TestClasses;
    using Xunit;

    public class WhenApplyingMapperConfigurations
    {
        [Fact]
        public void ShouldApplyAGivenMapperConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseConfigurations.From<PfiToPfsMapperConfiguration>();

                PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
            }
        }

        [Fact]
        public void ShouldApplyMapperConfigurationsInAGivenTypeAssembly()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseConfigurations.FromAssemblyOf<WhenApplyingMapperConfigurations>();

                PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
            }
        }

        [Fact]
        public void ShouldProvideRegisteredServicesToMapperConfigurations()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var mappersByName = new Dictionary<string, IMapper>();

                mapper.WhenMapping
                    .UseConfigurations.FromAssemblyOf<AnimalBase>(mappersByName);

                ServiceDictionaryMapperConfiguration
                    .VerifyConfigured(mappersByName)
                    .ShouldBeTrue();
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

        // ReSharper disable once ClassNeverInstantiated.Local
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
