namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringObjectMappingIncorrectly
    {
        [Fact]
        public void ShouldErrorIfSingleParameterObjectFactorySpecifiedWithInvalidParameter()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<DateTime, Address> addressFactory = dt => new Address();

                    mapper.WhenMapping
                        .From<Address>()
                        .To<Address>()
                        .MapInstancesUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfTwoParameterObjectFactorySpecifiedWithInvalidParameters()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<int, string, Address> addressFactory = (i, str) => new Address();

                    mapper.WhenMapping
                        .From<Address>()
                        .To<Address>()
                        .MapInstancesUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfThreeParameterObjectFactorySpecifiedWithInvalidParameters()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<CustomerViewModel, Customer, int?, CustomerViewModel> customerFactory =
                        (srcCVm, tgtC, i) => new CustomerViewModel { Name = srcCVm.Name };

                    mapper.WhenMapping
                        .From<CustomerViewModel>()
                        .To<Customer>()
                        .MapInstancesUsing(customerFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfFourParameterObjectFactorySpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<object, object, int?, Address, Address> addressFactory = (i, str, dt, ts) => new Address();

                    mapper.WhenMapping
                        .From<Address>()
                        .To<Address>()
                        .MapInstancesUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfConflictingMappingFactoriesConfigured()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Customer>()
                        .ToANew<Customer>()
                        .MapInstancesOf<Address>()
                        .Using(ctx => new Address { Line1 = "Hello!" });

                    mapper.WhenMapping
                        .From<Customer>()
                        .ToANew<Customer>()
                        .MapInstancesOf<Address>()
                        .Using(ctx => new Address { Line1 = "Hello!" });
                }
            });

            factoryEx.Message.ShouldContain("has already been configured");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/114
        [Fact]
        public void ShouldErrorIfPrimitiveTargetTypeSpecified()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<string>()
                        .To<long>()
                        .MapInstancesUsing(ctx => 123L);
                }
            });

            factoryEx.Message.ShouldContain("primitive type 'long'");
        }

        [Fact]
        public void ShouldErrorIfMappingFactoryConflictsWithCreationFactory()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .CreateInstancesUsing(ctx => new Address { Line1 = "Mapping!" });

                    mapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .MapInstancesUsing(ctx => new Address { Line1 = "Mapping!" });
                }
            });

            configEx.Message.ShouldContain("An object factory");
            configEx.Message.ShouldContain("already been configured");
        }
    }
}