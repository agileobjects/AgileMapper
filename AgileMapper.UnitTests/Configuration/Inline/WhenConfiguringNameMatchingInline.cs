namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringNameMatchingInline
    {
        [Fact]
        public void ShouldUseACustomPrefixInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new { _pValue = "Help!" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .UseNamePrefix("_p"));

                result.Value.ShouldBe("Help!");
            }
        }

        [Fact]
        public void ShouldUseMultipleCustomPrefixesInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(new { _fValue = "Oops!" })
                    .Over(new PublicField<string>(), cfg => cfg
                        .UseNamePrefixes("_p", "_f"));

                result1.Value.ShouldBe("Oops!");

                var result2 = mapper
                    .Map(new { _pValue = "D'oh!" })
                    .OnTo(new PublicField<string>(), cfg => cfg
                        .UseNamePrefixes("_f", "_p"));

                result2.Value.ShouldBe("D'oh!");

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldHandleACustomSuffixInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new { ValueStr = "La la la!" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .UseNameSuffix("Str"));

                result.Value.ShouldBe("La la la!");
            }
        }

        [Fact]
        public void ShouldHandleMultipleCustomSuffixesInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNameSuffixes("Str", "Int");

                var source = new { ValueInt = 12345 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("12345");
            }
        }

        [Fact]
        public void ShouldHandleACustomNamingPatternInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new { _abcValuexyz_ = 999 })
                    .ToANew<PublicField<string>>(cfg => cfg
                        .UseNamePattern("^_abc(.+)xyz_$"));

                result.Value.ShouldBe("999");
            }
        }

        [Fact]
        public void ShouldHandleCustomNamingPatternsInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new { __Value__ = 456 })
                    .ToANew<PublicField<int>>(cfg => cfg
                        .UseNamePatterns("^_abc(.+)xyz_$", "^__(.+)__$"));

                result.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldExtendMatchingPatternsInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePrefix("_p");

                var result1 = mapper
                    .Map(new { _pValue1 = "Prefix!", Value2Str_ = "Suffix!" })
                    .ToANew<PublicTwoFields<string, string>>(cfg => cfg
                        .UseNameSuffix("Str_"));

                result1.Value1.ShouldBe("Prefix!");
                result1.Value2.ShouldBe("Suffix!");

                var result2 = mapper
                    .Map(new { _pValue1 = "Prefix!", Value2__ = "Suffix again!" })
                    .ToANew<PublicTwoFields<string, string>>(cfg => cfg
                        .UseNameSuffix("__"));

                result2.Value1.ShouldBe("Prefix!");
                result2.Value2.ShouldBe("Suffix again!");

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldErrorIfInlinePatternHasNoPrefixOrSuffix()
        {
            var patternEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new { __Value__ = 456 })
                        .ToANew<PublicField<int>>(cfg => cfg
                            .UseNamePattern("(.+)"));
                }
            });

            patternEx.Message.ShouldContain("Name pattern '^(.+)$' is not valid.");
        }
    }
}
