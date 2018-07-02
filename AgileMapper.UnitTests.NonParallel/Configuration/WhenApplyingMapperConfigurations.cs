namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using System.Collections.Generic;
    using MoreTestClasses;
    using Xunit;
    using UnitTestsMapperConfigurations = UnitTests.Configuration.WhenApplyingMapperConfigurations;

    public class WhenApplyingMapperConfigurations : NonParallelTestsBase
    {
        [Fact]
        public void ShouldApplyMapperConfigurationsInGivenAssembliesViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                var mappersByName = new Dictionary<string, IMapper>();

                Mapper.WhenMapping
                    .UseConfigurations
                    .FromAssemblyOf<UnitTestsMapperConfigurations>()
                    .FromAssemblyOf<AnimalBase>(mappersByName);

                UnitTestsMapperConfigurations.PfiToPfsMapperConfiguration.VerifyConfigured(Mapper.Default);
                UnitTestsMapperConfigurations.PfsToPfiMapperConfiguration.VerifyConfigured(Mapper.Default);

                ServiceDictionaryMapperConfiguration
                    .VerifyConfigured(mappersByName)
                    .ShouldBeTrue();
            });
        }

        [Fact]
        public void ShouldProvideRegisteredServicesToMapperConfigurationsViaTheStaticApi()
        {
            var mappersByName = new Dictionary<string, IMapper>();

            TestThenReset(() =>
            {
                Mapper.WhenMapping
                    .UseConfigurations.From<ServiceDictionaryMapperConfiguration>(mappersByName);

                ServiceDictionaryMapperConfiguration
                    .VerifyConfigured(mappersByName)
                    .ShouldBeTrue();
            });
        }
    }
}
