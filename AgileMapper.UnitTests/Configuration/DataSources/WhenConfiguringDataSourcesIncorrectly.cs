namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                        .To(pf => pf.Value);
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
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicField<string>>()
                        .Map(pp => pp.Value, pf => pf.Value);
                }
            });

            configEx.Message.ShouldContain("PublicProperty<int>.Value");
            configEx.Message.ShouldContain("PublicField<string>.Value");
            configEx.Message.ShouldContain("does not need to be configured");
        }

        [Fact]
        public void ShouldErrorIfRedundantConstructorParameterDataSourceIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<string>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToCtor<string>();
                }
            });

            configEx.Message.ShouldContain("PublicProperty<int>.Value");
            configEx.Message.ShouldContain("will automatically be mapped");
            configEx.Message.ShouldContain("target constructor parameter");
            configEx.Message.ShouldContain("PublicCtor<string>.value");
            configEx.Message.ShouldContain("does not need to be configured");
        }

        [Fact]
        public void ShouldErrorIfRedundantDerivedTypeDataSourceIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
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

            configEx.Message.ShouldContain("already has configured data source");
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

            conflictEx.Message.ShouldContain("already has configured data source Person.Id");
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
        public void ShouldErrorIfUnconvertibleConstructorSourceValueSpecified()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicCtor<Guid>>()
                        .Map(ctx => ctx.Source.Value)
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
        public void ShouldErrorIfSimpleTypeConfiguredForComplexConstructorParameter()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>()
                        .To<PublicCtor<Address>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToCtor<Address>();
                }
            });

            configurationException.Message.ShouldContain(
                "PublicField<int>.Value of type 'int' cannot be mapped to target type 'Address'");
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
        public void ShouldErrorIfSimpleTypeConfiguredForEnumerableConstructorParameter()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<string>>()
                        .To<PublicCtor<int[]>>()
                        .Map(ctx => ctx.Source.Value)
                        .ToCtor("value");
                }
            });

            configurationException.Message.ShouldContain(
                "PublicField<string>.Value of type 'string' cannot be mapped to target type 'int[]'");
        }

        [Fact]
        public void ShouldErrorIfUnconvertibleEnumerableElementTypeConfigured()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<PublicField<int>[], int[]>>()
                        .To<PublicField<int[]>>()
                        .Map(s => s.Value1, t => t.Value);
                }
            });

            configurationException.Message.ShouldContain(
                "Unable to convert configured 'PublicField<int>' to target type 'int'");
        }

        [Fact]
        public void ShouldErrorIfConstantSpecifiedForTargetMember()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<Customer>>()
                        .To<PublicProperty<Customer>>()
                        .Map(ctx => "Number!")
                        .To(ppc => "Number?");
                }
            });

            configurationException.Message.ShouldContain("Unable to determine target member from '\"Number?\"'");
        }

        [Fact]
        public void ShouldErrorIfProjectionSpecifiedForTargetMember()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<IEnumerable<Customer>>>()
                        .To<PublicProperty<IEnumerable<Customer>>>()
                        .Map(ctx => new[] { "One", "Two", "Three" })
                        .To(ppc => ppc.Value.Select(c => c.Name));
                }
            });

            configurationException.Message.ShouldContain("not writeable");
        }

        [Fact]
        public void ShouldErrorIfBaseClassOnlySpecifiedForSealedType()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicSealed<int>>().ButNotDerivedTypes
                        .To<PublicField<int>>();
                }
            });

            configurationException.Message.ShouldContain("sealed");
        }
    }
}