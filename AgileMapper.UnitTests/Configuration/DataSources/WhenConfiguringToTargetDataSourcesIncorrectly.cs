namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using System;
    using System.Collections.Generic;
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
    public class WhenConfiguringToTargetDataSourcesIncorrectly
    {
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
        public void ShouldErrorIfSimpleTypeConstantConfiguredForRootTarget()
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
        public void ShouldErrorIfSimpleTypeMemberConfiguredForRootTarget()
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
        public void ShouldErrorIfSimpleTypeFactoryFuncConfiguredForRootTarget()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                Func<PublicProperty<int>, PublicField<long>, int> getValue = (pp, pf) => pp.Value;

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<int>>()
                        .To<PublicField<long>>()
                        .Map(getValue)
                        .ToTargetInstead();
                }
            });

            configurationException.Message.ShouldContain("Func<PublicProperty<int>, PublicField<long>, int>");
            configurationException.Message.ShouldContain("cannot be mapped to target type 'PublicField<long>'");
        }

        [Fact]
        public void ShouldErrorIfNonEnumerableTypeMemberConfiguredForRootEnumerableTarget()
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
        public void ShouldErrorIfEnumerableTypeMemberConfiguredForRootNonEnumerableTarget()
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

        [Fact]
        public void ShouldErrorIfUnconvertibleEnumerableElementTypeConfiguredForRootTarget()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<PublicField<decimal>[]>>()
                        .To<decimal[]>()
                        .Map(ctx => ctx.Source.Value)
                        .ToTarget();
                }
            });

            configurationException.Message.ShouldContain(
                "Unable to convert configured 'PublicField<decimal>' to target type 'decimal'");
        }

        [Fact]
        public void ShouldErrorIfConflictingToTargetAndToTargetInstead()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<int, PublicField<int>>>()
                        .ToANew<PublicField<int>>()
                        .Map((ptf, pf) => ptf.Value2).ToTarget()
                        .And
                        .Map((ptf, pf) => ptf.Value2).ToTargetInstead();
                }
            });

            configEx.Message.ShouldContain("already has configured ToTarget() data source");
            configEx.Message.ShouldContain("PublicTwoFields<int, PublicField<int>>.Value2");
        }

        [Fact]
        public void ShouldErrorIfConflictingToTargetInsteadAndToTarget()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<int, PublicField<int>>>()
                        .ToANew<PublicField<int>>()
                        .Map((ptf, pf) => ptf.Value2).ToTargetInstead()
                        .And
                        .Map((ptf, pf) => ptf.Value2).ToTarget();
                }
            });

            configEx.Message.ShouldContain("already has configured ToTargetInstead() data source");
            configEx.Message.ShouldContain("PublicTwoFields<int, PublicField<int>>.Value2");
        }
    }
}