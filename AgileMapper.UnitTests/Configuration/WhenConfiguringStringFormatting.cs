namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Common;
    using Common.TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringStringFormatting
    {
        // See https://github.com/agileobjects/AgileMapper/issues/23
        [Fact]
        public void ShouldFormatDateTimesMapperWide()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .StringsFrom<DateTime>(c => c.FormatUsing("o"));

                var source = new PublicProperty<DateTime> { Value = DateTime.Now };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(source.Value.ToString("o"));
            }
        }

        [Fact]
        public void ShouldFormatDecimalsMapperWide()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .StringsFrom<decimal>(c => c.FormatUsing("C"));

                var source = new PublicProperty<decimal> { Value = 1.99m };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(source.Value.ToString("C"));
            }
        }

        [Fact]
        public void ShouldFormatDoublesMapperWideUsingDecimalPlaces()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .StringsFrom<double>(c => c.FormatUsing("0.00"));

                var source = new PublicProperty<double> { Value = 1 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("1.00");
            }
        }

        [Fact]
        public void ShouldErrorIfNoFormatSpecified()
        {
            var noFormatEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .StringsFrom<double>(c => { });
                }
            });

            noFormatEx.Message.ShouldContain("No format string specified");
        }

        [Fact]
        public void ShouldErrorIfUnformattableTypeSpecified()
        {
            var noFormatEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .StringsFrom<PublicField<string>>(c => c.FormatUsing("xxx"));
                }
            });

            noFormatEx.Message.ShouldContain("No ToString method");
        }
    }
}
