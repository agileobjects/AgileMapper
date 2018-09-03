namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringObjectTrackingIncorrectly
    {
        [Fact]
        public void ShouldErrorIfGlobalIdentityIntegrityConfiguredWithDisabledObjectTracking()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping.MaintainIdentityIntegrity();
                });

                configEx.Message.ShouldContain("Identity integrity cannot be configured");
                configEx.Message.ShouldContain("global object tracking disabled");
            }
        }

        [Fact]
        public void ShouldErrorIfGlobalIdentityIntegrityConfiguredTwice()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping.MaintainIdentityIntegrity();
                });

                configEx.Message.ShouldContain("Identity integrity is already configured");
            }
        }

        [Fact]
        public void ShouldErrorIfIdentityIntegrityConfiguredTwice()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .To<ProductDto>()
                        .MaintainIdentityIntegrity();
                });

                configEx.Message.ShouldContain("Identity integrity is already configured");
                configEx.Message.ShouldContain("Product -> ProductDto");
            }
        }

        [Fact]
        public void ShouldErrorIfRedundantIdentityIntegrityConfigured()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping
                        .From<MegaProduct>()
                        .To<ProductDtoMega>()
                        .MaintainIdentityIntegrity();
                });

                configEx.Message.ShouldContain("Identity integrity is already configured");
                configEx.Message.ShouldContain("Product -> ProductDto");
            }
        }

        [Fact]
        public void ShouldErrorIfGlobalObjectTrackingDisabledWithIdentityIntegrityConfigured()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping.DisableObjectTracking();
                });

                configEx.Message.ShouldContain("Object tracking cannot be disabled");
                configEx.Message.ShouldContain("global identity integrity configured");
            }
        }

        [Fact]
        public void ShouldErrorIfGlobalObjectTrackingDisabledTwice()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping.DisableObjectTracking();
                });

                configEx.Message.ShouldContain("Object tracking is already disabled");
            }
        }

        [Fact]
        public void ShouldErrorIfObjectTrackingDisabledTwice()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .DisableObjectTracking();

                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping
                        .From<PersonViewModel>()
                        .To<Person>()
                        .DisableObjectTracking();
                });

                configEx.Message.ShouldContain("Object tracking is already disabled");
                configEx.Message.ShouldContain("PersonViewModel -> Person");
            }
        }
    }
}
