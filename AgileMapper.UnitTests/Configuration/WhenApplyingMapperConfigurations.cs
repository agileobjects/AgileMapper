namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
#if NET40
    using System;
#endif
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using MoreTestClasses;
    using NetStandardPolyfills;
    using TestClasses;
    using Xunit;

    public class WhenApplyingMapperConfigurations : AssemblyScanningTestClassBase
    {
        [Fact]
        public void ShouldApplyAGivenMapperConfiguration()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseConfigurations.From<PfiToPfsMapperConfiguration>();

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                }
            });
        }

        [Fact]
        public void ShouldApplyMapperConfigurationsInAGivenTypeAssembly()
        {
            TestThenReset(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping
                            .UseConfigurations.FromAssemblyOf<WhenApplyingMapperConfigurations>();

                        PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                        PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
                    }
                });
        }

        [Fact]
        public void ShouldProvideRegisteredServicesToMapperConfigurations()
        {
            TestThenReset(() =>
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
            });
        }

#if NET40
        [Fact]
        public void ShouldApplyMapperConfigurationsFromGivenAssemblies()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var mappersByName = new Dictionary<string, IMapper>();

                    mapper.WhenMapping
                        .UseConfigurations.From(AppDomain.CurrentDomain.GetAssemblies(), mappersByName);

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                    PfsToPfiMapperConfiguration.VerifyConfigured(mapper);

                    ServiceDictionaryMapperConfiguration
                        .VerifyConfigured(mappersByName)
                        .ShouldBeTrue();
                }
            });
        }
#endif

        [Fact]
        public void ShouldFilterMapperConfigurationsFromGivenAssemblies()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseConfigurations.From(
                        new[]
                        {
                        typeof(PfiToPfsMapperConfiguration).GetAssembly(),
                        typeof(ServiceDictionaryMapperConfiguration).GetAssembly()
                        },
                        assembly => !assembly.FullName.Contains(nameof(MoreTestClasses)));

                    PfiToPfsMapperConfiguration.VerifyConfigured(mapper);
                    PfsToPfiMapperConfiguration.VerifyConfigured(mapper);
                }
            });
        }

        #region Helper Classes

        public class PfiToPfsMapperConfiguration : MapperConfiguration
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
        public class PfsToPfiMapperConfiguration : MapperConfiguration
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
