namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

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
                        .Map((p, x) => p.Name)
                        .To(x => x.Value);
                }
            });
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
                    mapper
                        .WhenMapping
                        .From<PublicProperty<string>>()
                        .To<PublicSetMethod<string>>()
                        .Map(ctx => ctx.Source.Value)
                        .To(psm => psm.Value);
                }
            });
        }
    }
}