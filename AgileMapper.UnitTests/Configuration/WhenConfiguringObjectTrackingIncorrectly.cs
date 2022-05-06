namespace AgileObjects.AgileMapper.UnitTests.Configuration;

using AgileMapper.Configuration;
using Common;
using Common.TestClasses;
using TestClasses;
#if !NET35
using Xunit;
#else
using Fact = NUnit.Framework.TestAttribute;

[NUnit.Framework.TestFixture]
#endif
[Trait("Category", "Checked")]
public class WhenConfiguringObjectTrackingIncorrectly
{
    [Fact]
    public void ShouldErrorIfGlobalIdentityIntegrityConfiguredWithDisabledObjectTracking()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();

                mapper.WhenMapping.MaintainIdentityIntegrity();
            }
        });

        configEx.Message.ShouldContain("Identity integrity cannot be configured");
        configEx.Message.ShouldContain("global object tracking disabled");
    }

    [Fact]
    public void ShouldErrorIfGlobalIdentityIntegrityConfiguredTwice()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();
                mapper.WhenMapping.MaintainIdentityIntegrity();
            }
        });

        configEx.Message.ShouldContain("Identity integrity is already configured");
    }

    [Fact]
    public void ShouldErrorIfIdentityIntegrityConfiguredTwice()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();

                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();
            }
        });

        configEx.Message.ShouldContain("Identity integrity is already configured");
        configEx.Message.ShouldContain("Product -> ProductDto");
    }

    [Fact]
    public void ShouldErrorIfRedundantIdentityIntegrityConfigured()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();

                mapper.WhenMapping
                    .From<MegaProduct>()
                    .To<ProductDtoMega>()
                    .MaintainIdentityIntegrity();
            }
        });

        configEx.Message.ShouldContain("Identity integrity is already configured");
        configEx.Message.ShouldContain("Product -> ProductDto");
    }

    [Fact]
    public void ShouldErrorIfGlobalObjectTrackingDisabledWithIdentityIntegrityConfigured()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();
                mapper.WhenMapping.DisableObjectTracking();
            }
        });

        configEx.Message.ShouldContain("Object tracking cannot be disabled");
        configEx.Message.ShouldContain("global identity integrity configured");
    }

    [Fact]
    public void ShouldErrorIfGlobalObjectTrackingDisabledTwice()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();
                mapper.WhenMapping.DisableObjectTracking();
            }
        });

        configEx.Message.ShouldContain("Object tracking is already disabled");
    }

    [Fact]
    public void ShouldErrorIfDuplicateObjectTrackingDisabled()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>().To<Person>()
                    .DisableObjectTracking();

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .DisableObjectTracking();
            }
        });

        configEx.Message.ShouldContain("Object tracking is already disabled");
        configEx.Message.ShouldContain("PersonViewModel -> Person");
    }

    [Fact]
    public void ShouldErrorIfRedundantObjectTrackingDisabled()
    {
        var configEx = Should.Throw<MappingConfigurationException>(() =>
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<Person>()
                    .DisableObjectTracking();

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .DisableObjectTracking();
            }
        });

        configEx.Message.ShouldContain("Object tracking is already disabled");
        configEx.Message.ShouldContain("to Person");
    }
}