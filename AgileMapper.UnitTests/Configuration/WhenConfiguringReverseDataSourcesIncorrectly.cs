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
    public class WhenConfiguringReverseDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfRedundantMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .AutoReverseConfiguredDataSources()
                        .AndWhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("data source reversal");
            configEx.Message.ShouldContain("enabled by default");
        }

        [Fact]
        public void ShouldErrorIfRedundantMemberScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .AutoReverseConfiguredDataSources()
                        .AndWhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id)
                        .To(pp => pp.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("reversed");
            configEx.Message.ShouldContain("enabled by default");
        }

        [Fact]
        public void ShouldErrorIfRedundantMappingScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Customer>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources()
                        .And
                        .Map(c => c.Id, pp => pp.Value)
                        .ButNotViceVersa();
                }
            });

            configEx.Message.ShouldContain("reverse");
            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfRedundantMemberScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id)
                        .To(pp => pp.Value)
                        .ButNotViceVersa();
                }
            });

            configEx.Message.ShouldContain("reverse");
            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfRedundantReverseDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicProperty<Guid>>()
                        .Map(p => p.Id, pp => pp.Value);

                    mapper.WhenMapping
                        .From<PublicProperty<Guid>>()
                        .To<Person>()
                        .Map(pp => pp.Value, p => p.Id);
                }
            });

            configEx.Message.ShouldContain("reverse data source");
        }

        [Fact]
        public void ShouldErrorIfRedundantExplicitReverseDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicTwoFields<Guid, Guid>>()
                        .Map(p => p.Id, ptf => ptf.Value1)
                        .AndViceVersa();

                    mapper.WhenMapping
                        .From<PublicTwoFields<Guid, Guid>>()
                        .To<Person>()
                        .Map(pp => pp.Value2, p => p.Id);
                }
            });

            configEx.Message.ShouldContain("reverse data source");
        }

        [Fact]
        public void ShouldErrorOnMemberScopeOptInOfConfiguredConstantDataSource()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map("HELLO!").To(pf => pf.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("configured value '\"HELLO!\"'");
            configEx.Message.ShouldContain("not a source member");
        }

        [Fact]
        public void ShouldErrorOnMemberScopeOptInOfConfiguredReadOnlySourceMemberDataSource()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicReadOnlyField<string>>()
                        .To<CustomerViewModel>()
                        .Map((prof, cvm) => prof.Value).To(cvm => cvm.AddressLine1)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("source member 'PublicReadOnlyField<string>.Value'");
            configEx.Message.ShouldContain("not mappable");
            configEx.Message.ShouldContain("readonly string");
        }
    }
}