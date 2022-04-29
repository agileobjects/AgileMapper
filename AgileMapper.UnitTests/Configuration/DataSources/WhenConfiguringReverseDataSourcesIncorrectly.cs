namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
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
    [Trait("Category", "Checked")]
    public class WhenConfiguringReverseDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfGlobalScopeRedundantMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .AutoReverseConfiguredDataSources()
                        .AndWhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("data source reversal");
            configEx.Message.ShouldContain("enabled by default");
        }

        [Fact]
        public void ShouldErrorIfGlobalScopeRedundantMemberScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .AutoReverseConfiguredDataSources()
                        .AndWhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id).To(pp => pp.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("reversed");
            configEx.Message.ShouldContain("enabled by default");
        }

        [Fact]
        public void ShouldErrorIfMappingScopeRedundantMemberScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources()
                        .And
                        .Map(ctx => ctx.Source.Id).To(pp => pp.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("reversed");
            configEx.Message.ShouldContain("enabled by default");
        }

        [Fact]
        public void ShouldErrorIfMappingScopeRedundantDerivedMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                    
                    mapper.WhenMapping
                        .From<MegaProduct>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("already enabled");
            configEx.Message.ShouldContain("Product -> PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfGlobalScopeRedundantMappingScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("data source reversal");
            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfGlobalScopeRedundantMemberScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .Map(ctx => ctx.Source.Id).To(pp => pp.Value)
                        .ButNotViceVersa();
                }
            });

            configEx.Message.ShouldContain("reverse");
            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfMappingScopeRedundantMemberScopeOptOutConfigured()
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
        public void ShouldErrorIfDuplicateMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("already enabled");
            configEx.Message.ShouldContain("Person -> PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfAllSourcesConflictingMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("already enabled");
            configEx.Message.ShouldContain("to PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfConflictingMappingScopeOptInConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("cannot be disabled");
            configEx.Message.ShouldContain("already been enabled");
            configEx.Message.ShouldContain("Person -> PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfDuplicateMappingScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("already disabled");
            configEx.Message.ShouldContain("Person -> PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfAllSourcesConflictingMappingScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("already disabled");
            configEx.Message.ShouldContain("to PublicProperty<Guid>");
        }

        [Fact]
        public void ShouldErrorIfConflictingMappingScopeOptOutConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.AutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .DoNotAutoReverseConfiguredDataSources();

                    mapper.WhenMapping
                        .From<Person>().To<PublicProperty<Guid>>()
                        .AutoReverseConfiguredDataSources();
                }
            });

            configEx.Message.ShouldContain("cannot be enabled");
            configEx.Message.ShouldContain("already been disabled");
            configEx.Message.ShouldContain("Person -> PublicProperty<Guid>");
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
                        .From<Person>().To<PublicProperty<Guid>>()
                        .Map(p => p.Id, pp => pp.Value);

                    mapper.WhenMapping
                        .From<PublicProperty<Guid>>().To<Person>()
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
                        .From<Person>().To<PublicTwoFields<Guid, Guid>>()
                        .Map(p => p.Id, ptf => ptf.Value1)
                        .AndViceVersa();

                    mapper.WhenMapping
                        .From<PublicTwoFields<Guid, Guid>>().To<Person>()
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
                        .From<Person>().To<PublicField<string>>()
                        .Map("HELLO!").To(pf => pf.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("configured value '\"HELLO!\"'");
            configEx.Message.ShouldContain("not a source member");
        }

        [Fact]
        public void ShouldErrorOnMemberScopeOptInOfConfiguredConstantFuncDataSource()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicField<string>>()
                        .Map(p => "HELLO?!", pf => pf.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("configured value '\"HELLO?!\"'");
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
                        .From<PublicReadOnlyField<string>>().To<CustomerViewModel>()
                        .Map((prof, cvm) => prof.Value).To(cvm => cvm.AddressLine1)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("source member 'PublicReadOnlyField<string>.Value'");
            configEx.Message.ShouldContain("not mappable");
            configEx.Message.ShouldContain("readonly string");
        }

        [Fact]
        public void ShouldErrorOnMemberScopeOptInOfConfiguredSourceMemberDataSourceForWriteOnlyTarget()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<decimal, decimal>>().To<PublicWriteOnlyProperty<int>>()
                        .Map((pp, pwop) => pp.Value1).To(pwop => pwop.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("target member 'PublicWriteOnlyProperty<int>.Value'");
            configEx.Message.ShouldContain("not readable");
            configEx.Message.ShouldContain("cannot be used as a source member");
        }

        [Fact]
        public void ShouldErrorOnMemberScopeOptInOfConditionalConfiguredDataSource()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>().To<PublicField<string>>()
                        .If((p, pf) => p.Name.Contains("Rich"))
                        .Map(p => p.Name, pf => pf.Value)
                        .AndViceVersa();
                }
            });

            configEx.Message.ShouldContain("cannot be reversed");
            configEx.Message.ShouldContain("has condition");
            configEx.Message.ShouldContain("p.Name.Contains(\"Rich\")");
        }
    }
}