﻿namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Reflection;
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
    using static WhenConfiguringDerivedTypes;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDerivedTypesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfNoAssembliesToScanSupplied()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.LookForDerivedTypesIn();
                }
            });

            configEx.Message.ShouldContain("assemblies must be specified");
            configEx.InnerException.ShouldBeOfType<ArgumentException>();
        }

        [Fact]
        public void ShouldErrorIfNullAssemblyToScanSupplied()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.LookForDerivedTypesIn(default(Assembly));
                }
            });

            configEx.Message.ShouldContain("assemblies must be non-null");
            configEx.InnerException.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public void ShouldErrorIfSameSourceTypeSpecified()
        {
            var pairingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .To<ProductDto>()
                        .Map<Product>()
                        .To<ProductDtoMega>();

                }
            });

            pairingEx.Message.ShouldContain("derived source type must be specified");
        }

        [Fact]
        public void ShouldErrorIfSameTargetTypeSpecified()
        {
            var pairingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .To<ProductDto>()
                        .Map<MegaProduct>()
                        .To<ProductDto>();

                }
            });

            pairingEx.Message.ShouldContain("derived target type must be specified");
        }

        [Fact]
        public void ShouldErrorIfUnnecessaryPairingSpecified()
        {
            var pairingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Map<Customer>()
                        .To<CustomerViewModel>();

                }
            });

            pairingEx.Message.ShouldContain("Customer is automatically mapped to CustomerViewModel");
            pairingEx.Message.ShouldContain("when mapping Person to PersonViewModel");
            pairingEx.Message.ShouldContain("does not need to be configured");
        }

        [Fact]
        public void ShouldErrorIfInterfaceTypeSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                        .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                        .MapTo<Issue123.ILeaf>();
                }
            });
        }
    }
}