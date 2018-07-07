namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using System.Reflection;
    using AgileMapper.Configuration;
    using MoreTestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenApplyingMapperConfigurationsIncorrectly : AssemblyScanningTestClassBase
    {
        [Fact]
        public void ShouldErrorIfNullAssemblyCollectionSupplied()
        {
            TestThenReset(() =>
            {
                var configError = Should.Throw<MappingConfigurationException>(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping.UseConfigurations.From(default(IEnumerable<Assembly>));
                    }
                });

                configError.Message.ShouldContain("cannot be null");
            });
        }

        [Fact]
        public void ShouldErrorIfEmptyAssemblyCollectionSupplied()
        {
            TestThenReset(() =>
            {
                var configError = Should.Throw<MappingConfigurationException>(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping.UseConfigurations.From(Enumerable<Assembly>.Empty);
                    }
                });

                configError.Message.ShouldContain("cannot be empty");
            });
        }

        [Fact]
        public void ShouldErrorIfNullAssemblySupplied()
        {
            TestThenReset(() =>
            {
                var configError = Should.Throw<MappingConfigurationException>(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping.UseConfigurations.From(new[] { default(Assembly) });
                    }
                });

                configError.Message.ShouldContain("assemblies must be non-null");
            });
        }

        [Fact]
        public void ShouldWrapAMapperConfigurationError()
        {
            TestThenReset(() =>
            {
                var configError = Should.Throw<MappingConfigurationException>(() =>
                {
                    using (var mapper = Mapper.CreateNew())
                    {
                        mapper.WhenMapping
                            .UseConfigurations.FromAssemblyOf<Dog>();
                    }
                });

                configError.Message.ShouldContain("Exception encountered");
                configError.Message.ShouldContain(nameof(ServiceDictionaryMapperConfiguration));
            });
        }
    }
}