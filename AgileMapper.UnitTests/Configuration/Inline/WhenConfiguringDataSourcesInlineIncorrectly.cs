namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSourcesInlineIncorrectly
    {
        [Fact]
        public void ShouldErrorIfUnconvertibleConstantSpecifiedInline()
        {
            var inlineConfigEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new PublicField<DateTime> { Value = DateTime.Now })
                        .ToANew<PublicField<DateTime>>(cfg => cfg
                            .Map((decimal?)63762m)
                            .To(pf => pf.Value));
                }
            });

            inlineConfigEx.Message.ShouldContain("Unable to convert configured decimal? ");
        }

        [Fact]
        public void ShouldErrorIfDuplicateDataSourceIsConfiguredInline()
        {
            var inlineConfigEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<string>>()
                        .To<PublicField<string>>()
                        .Map(ctx => ctx.Source.Value + "!")
                        .To(pf => pf.Value);

                    mapper
                        .Map(new PublicField<string> { Value = "Hello" })
                        .ToANew<PublicField<string>>(cfg => cfg
                            .Map(ctx => ctx.Source.Value + "!")
                            .To(pf => pf.Value));
                }
            });

            inlineConfigEx.Message.ShouldContain("already has that configured data source");
        }

        [Fact]
        public void ShouldErrorIfIgnoredMemberHasDataSourceConfiguredInline()
        {
            var inlineConfigEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new PublicField<string> { Value = "Hello" })
                        .ToANew<PublicField<string>>(
                            cfg => cfg
                                .Ignore(pf => pf.Value),
                            cfg => cfg
                                .Map(ctx => ctx.Source.Value + "?!")
                                .To(pf => pf.Value));
                }
            });

            inlineConfigEx.Message.ShouldContain("Ignored member Target.Value has a configured data source");
        }

        [Fact]
        public void ShouldErrorIfMissingConstructorParameterTypeSpecifiedInline()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new PublicProperty<int> { Value = 2 })
                        .ToANew<PublicCtorStruct<int>>(cfg => cfg
                            .Map((s, t, i) => i)
                            .ToCtor<long>());
                }
            });

            configurationException.Message.ShouldContain("No constructor parameter of type");
        }

        [Fact]
        public void ShouldErrorIfMissingConstructorParameterNameSpecifiedInline()
        {
            var configurationException = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new PublicProperty<int> { Value = 2 })
                        .ToANew<PublicCtorStruct<int>>(cfg => cfg
                            .Map((s, t, i) => i)
                            .ToCtor("Value"));
                }
            });

            configurationException.Message.ShouldContain("No constructor parameter named");
        }
    }
}
