namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringStringFormattingInline
    {
        [Fact]
        public void ShouldFormatDoublesWithDecimalPlacesInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(new PublicProperty<double> { Value = 1 })
                    .ToANew<PublicField<string>>(cfg => cfg
                        .WhenMapping
                        .StringsFrom<double>(c => c.FormatUsing("0.000")));

                result1.Value.ShouldBe("1.000");

                var result2 = mapper
                    .Map(new PublicProperty<double> { Value = 1 })
                    .ToANew<PublicField<string>>();

                result2.Value.ShouldBe("1");

                var result3 = mapper
                    .Map(new PublicProperty<double> { Value = 1 })
                    .ToANew<PublicField<string>>(cfg => cfg
                        .WhenMapping
                        .StringsFrom<double>(c => c.FormatUsing("0.00")));

                result3.Value.ShouldBe("1.00");

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldErrorIfUnformattableTypeSpecifiedInline()
        {
            var noFormatEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new { Value = 123 })
                        .Over(new PublicField<long>(), cfg => cfg.WhenMapping
                            .StringsFrom<PublicField<string>>(c => c.FormatUsing("xxx")));
                }
            });

            noFormatEx.Message.ShouldContain("No ToString method");
        }
    }
}
