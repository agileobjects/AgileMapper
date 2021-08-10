namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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
    public class WhenConfiguringObjectCreationIncorrectly
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
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
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
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
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
                        .CreateInstancesUsing(customerFactory);
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
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfConflictingCreationFactoriesConfigured()
        {
            var factoryEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address { Line1 = "Hello!" });

                    mapper.WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(ctx => new Address { Line1 = "Hello!" });
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
                        .To<int>()
                        .CreateInstancesUsing(ctx => 123);
                }
            });

            factoryEx.Message.ShouldContain("primitive type 'int'");
        }

        [Fact]
        public void ShouldErrorIfCreationFactoryConflictsWithMappingFactory()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .MapInstancesUsing(ctx => new Address { Line1 = "Mapping!" });

                    mapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .CreateInstancesUsing(ctx => new Address { Line1 = "Mapping!" });
                }
            });

            configEx.Message.ShouldContain("A mapping factory");
            configEx.Message.ShouldContain("already been configured");
        }
    }
}