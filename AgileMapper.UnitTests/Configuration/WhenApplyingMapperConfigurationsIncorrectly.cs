namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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
            var configError = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseConfigurations.From(default(IEnumerable<Assembly>));
                }
            });

            configError.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfEmptyAssemblyCollectionSupplied()
        {
            var configError = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseConfigurations.From(Enumerable<Assembly>.Empty);
                }
            });

            configError.Message.ShouldContain("cannot be empty");
        }

        [Fact]
        public void ShouldErrorIfNullAssemblySupplied()
        {
            var configError = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseConfigurations.From(new[] { default(Assembly) });
                }
            });

            configError.Message.ShouldContain("assemblies must be non-null");
        }

        [Fact]
        public void ShouldErrorIfDependentConfigurationAddedBeforeDependedOnConfiguration()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseConfigurations
                        .From<WhenApplyingMapperConfigurations.ParentMapperConfiguration>()
                        .From<WhenApplyingMapperConfigurations.ChildMapperConfiguration>();
                }
            });

            configEx.Message.ShouldContain("ParentMapperConfiguration");
            configEx.Message.ShouldContain("ChildMapperConfiguration");
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

        [Fact]
        public void ShouldErrorIfDependencyAttributeGivenNoTypes()
        {
            Should.Throw<MappingConfigurationException>(() =>
                new ApplyAfterAttribute());
        }

        [Fact]
        public void ShouldErrorIfDependencyAttributeGivenNullType()
        {
            Should.Throw<MappingConfigurationException>(() =>
                new ApplyAfterAttribute(default(Type)));
        }

        [Fact]
        public void ShouldErrorIfDependencyAttributeGivenNonConfigurationType()
        {
            Should.Throw<MappingConfigurationException>(() =>
                new ApplyAfterAttribute(typeof(string)));
        }
    }
}