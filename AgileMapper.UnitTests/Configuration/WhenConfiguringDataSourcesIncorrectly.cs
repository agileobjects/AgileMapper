namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfUnconvertibleConstantSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>()
                        .To<PublicField<DateTime>>()
                        .Map(new byte[] { 2, 4, 6, 8 })
                        .To(x => x.Value);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfIgnoredMemberIsConfigured()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>()
                        .To<PublicField<DateTime>>()
                        .Ignore(pf => pf.Value)
                        .But
                        .Map(ctx => DateTime.UtcNow)
                        .To(x => x.Value);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfConditionHasAnIsTypeTest()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .If((s, t) => s is Customer)
                        .Map((p, x) => "Customer " + p.Name)
                        .To(x => x.Value);
                }
            });

            mappingEx.Message.ShouldContain("Instead of type testing");
        }

        [Fact]
        public void ShouldErrorIfConditionHasAnAsTypeTest()
        {
            var mappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .If((s, t) => (s as Customer) != null)
                        .Map((p, x) => "Customer " + p.Name)
                        .To(x => x.Value);
                }
            });

            mappingEx.Message.ShouldContain("Instead of type testing");
        }

        [Fact]
        public void ShouldErrorIfDuplicateDataSourceIsConfigured()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Id)
                        .To(x => x.Value);

                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Id)
                        .To(x => x.Value);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfRedundantDataSourceIsConfigured()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Id)
                        .To(x => x.Value);

                    mapper.WhenMapping
                        .From<Customer>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Id)
                        .To(x => x.Value);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfConflictingDataSourceIsConfigured()
        {
            var conflictEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Id)
                        .To(x => x.Value);

                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, x) => p.Name)
                        .To(x => x.Value);
                }
            });

            conflictEx.Message.ShouldContain("already has a configured data source");
        }

        [Fact]
        public void ShouldNotErrorIfDerivedSourceTypeConflictingDataSourceIsConfigured()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicField<string>>()
                    .Map((p, x) => p.Id)
                    .To(x => x.Value);

                mapper.WhenMapping
                    .From<Customer>()
                    .To<PublicField<string>>()
                    .Map((p, x) => p.Name)
                    .To(x => x.Value);
            }
        }

        [Fact]
        public void ShouldErrorIfReadOnlyMemberSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<string>>()
                        .To<PublicSetMethod<string>>()
                        .Map(ctx => ctx.Source.Value)
                        .To(psm => psm.Value);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfMissingConstructorParameterTypeSpecified()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(Guid.NewGuid())
                        .ToCtor<string>();
                }
            });

            configurationException.Message.ShouldContain("No constructor parameter of type");
        }

        [Fact]
        public void ShouldErrorIfMissingConstructorParameterNameSpecified()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(Guid.NewGuid())
                        .ToCtor("boing");
                }
            });

            configurationException.Message.ShouldContain("No constructor parameter named");
        }

        [Fact]
        public void ShouldErrorIfNonUniqueConstructorParameterTypeSpecified()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicTwoParamCtor<DateTime, DateTime>>()
                        .Map(DateTime.Today)
                        .ToCtor<DateTime>();
                }
            });

            configurationException.Message.ShouldContain("Multiple constructor parameters");
        }

        [Fact]
        public void ShouldErrorIfUnconvertibleConstructorValueConstantSpecified()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(DateTime.Today)
                        .ToCtor<Guid>();
                }
            });

            configurationException.Message.ShouldContain("Unable to convert");
        }

        [Fact]
        public void ShouldErrorIfSimpleTypeConfiguredForComplexTarget()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<Person>()
                        .Map((s, t) => s.Id)
                        .To(t => t.Address);
                }
            });

            configurationException.Message.ShouldContain(
                "Person.Id of type 'Guid' cannot be mapped to target type 'Address'");
        }

        [Fact]
        public void ShouldErrorIfSimpleTypeConfiguredForEnumerableTarget()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>()
                        .To<PublicTwoFields<int[], int>>()
                        .Map((s, t) => s.Value)
                        .To(t => t.Value1);
                }
            });

            configurationException.Message.ShouldContain(
                "PublicField<int>.Value of type 'int' cannot be mapped to target type 'int[]'");
        }

        [Fact]
        public void ShouldErrorIfTargetParameterConfiguredAsTarget()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<Address>()
                        .Map((s, t) => s.Address)
                        .To(t => t);
                }
            });

            configurationException.Message.ShouldContain("not a valid configured target; use .ToTarget()");
        }

        [Fact]
        public void ShouldErrorIfRootTargetSimpleTypeConstantDataSourceConfigured()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicField<Guid>>()
                        .Map("No no no no no")
                        .ToTarget();
                }
            });

            configurationException.Message.ShouldContain(
                "'string' cannot be mapped to target type 'PublicField<Guid>'");
        }

        [Fact]
        public void ShouldErrorIfRootTargetSimpleTypeMemberDataSourceConfigured()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicField<long>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToTarget();
                }
            });

            configurationException.Message.ShouldContain("PublicProperty<int>.Value");
            configurationException.Message.ShouldContain("'int' cannot be mapped to target type 'PublicField<long>'");
        }

        [Fact]
        public void ShouldErrorIfRootEnumerableTargetNonEnumerableTypeMemberDataSourceConfigured()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<Address>>()
                        .To<List<Address>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToTarget();
                }
            });

            configurationException.Message.ShouldContain("Non-enumerable PublicProperty<Address>.Value");
            configurationException.Message.ShouldContain("cannot be mapped to enumerable target type 'List<Address>'");
        }

        [Fact]
        public void ShouldErrorIfRootNonEnumerableTargetEnumerableTypeMemberDataSourceConfigured()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<List<Customer>>>()
                        .To<PublicProperty<Customer>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToTarget();
                }
            });

            configurationException.Message.ShouldContain("Enumerable PublicField<List<Customer>>.Value");
            configurationException.Message.ShouldContain("cannot be mapped to non-enumerable target type 'PublicProperty<Customer>'");
        }
    }
}