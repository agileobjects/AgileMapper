namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringNameMatching
    {
        [Fact]
        public void ShouldHandleACustomPrefix()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePrefix("_p");

                var source = new { _pValue = "Help!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("Help!");
            }
        }

        [Fact]
        public void ShouldHandleMultipleCustomPrefixes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePrefixes("_p", "_f");

                var source = new { _fValue = "Oops!" };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("Oops!");
            }
        }

        [Fact]
        public void ShouldHandleACustomSuffix()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNameSuffix("Str");

                var source = new { ValueStr = "La la la!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("La la la!");
            }
        }

        [Fact]
        public void ShouldHandleMultipleCustomSuffixes()
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
        public void ShouldHandleACustomNamingPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePattern("^_abc(.+)xyz_$");

                var source = new { _abcValuexyz_ = 999 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("999");
            }
        }

        [Fact]
        public void ShouldHandleACustomNamingPrefixPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePattern("^__(.+)$");

                var source = new { __Value = 911 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("911");
            }
        }

        [Fact]
        public void ShouldHandleACustomNamingSuffixPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePattern("^(.+)__$");

                var source = new { Value__ = 878 };
                var result = mapper.Map(source).ToANew<PublicField<long>>();

                result.Value.ShouldBe(878);
            }
        }

        [Fact]
        public void ShouldHandleCustomNamingPatterns()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .UseNamePatterns("^_abc(.+)xyz_$", "^__(.+)__$");

                var source = new { __Value__ = 456 };
                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldErrorIfInvalidNamePatternFormatSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .WhenMapping
                        .UseNamePatterns("^_[Name]_$");
                }
            });
        }

        [Fact]
        public void ShouldErrorIfNamePatternContainsNewLine()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseNamePatterns(@"
^_
(.+)
_$");
                }
            });
        }

        [Fact]
        public void ShouldErrorIfNamePatternIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .UseNamePattern(null);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfNoPatternsSupplied()
        {
            Should.Throw<ArgumentException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .WhenMapping
                        .UseNamePatterns();
                }
            });
        }

        [Fact]
        public void ShouldErrorIfPatternHasNoPrefixOrSuffix()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .WhenMapping
                        .UseNamePattern("(.+)");
                }
            });
        }
    }
}
