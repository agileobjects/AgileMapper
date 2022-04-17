namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Common.TestClasses;
    using static System.Environment;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenConfiguringNameMatching
    {
        [Fact]
        public void ShouldUseACustomPrefix()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePrefix("_p");

                var source = new { _pValue = "Help!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("Help!");
            }
        }

        [Fact]
        public void ShouldUseMultipleCustomPrefixes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePrefixes("_p", "_f");

                var source = new { _fValue = "Oops!" };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("Oops!");
            }
        }

        [Fact]
        public void ShouldUseACustomSuffix()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNameSuffix("Str");

                var source = new { ValueStr = "La la la!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("La la la!");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/175
        [Fact]
        public void ShouldUseACustomSuffixForAllTypesExplicitly()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.From<object>().To<object>().UseNameSuffix("Str");

                var source = new { ValueStr = "La la la!" };
                var result = mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("La la la!");
            }
        }

        [Fact]
        public void ShouldUseMultipleCustomSuffixes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNameSuffixes("Str", "Int");

                var source = new { ValueInt = 12345 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("12345");
            }
        }

        [Fact]
        public void ShouldUseMultipleCustomSuffixesForGivenSourceAndTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { ValueInt = default(int) })
                    .To<PublicField<string>>()
                    .UseNameSuffixes("Str", "Int");

                var source = new { ValueInt = 12345 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("12345");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePattern("^_abc(.+)xyz_$");

                var source = new { _abcValuexyz_ = 999 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("999");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPatternForGivenSourceAndTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { _abcValuexyz_ = default(int) })
                    .To<PublicField<string>>()
                    .UseNamePattern("^_abc(.+)xyz_$");

                var source = new { _abcValuexyz_ = 999 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("999");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPatternInIdentifierMatching()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePattern("^_(.+)_$");

                var source = new[] { new { _Id_ = 123, _Price_ = 1.99 } };
                var target = new List<ProductDto>
                {
                    new ProductDto { ProductId = "123", Price = 0.99m }
                };
                var productDto = target.First();
                var result = mapper.Map(source).Over(target);

                result.ShouldHaveSingleItem();
                result.First().ShouldBeSameAs(productDto);
                result.First().Price.ShouldBe(1.99m);
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPrefixPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePattern("^__(.+)$");

                var source = new { __Value = 911 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("911");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPrefixPatternForGivenSourceAndTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { __Value = default(int) })
                    .To<PublicField<string>>()
                    .UseNamePattern("^__(.+)$");

                var source = new { __Value = 911 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("911");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingPrefixPatternForGivenSourceAndTargetTypesInANestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { _Value_ = default(int) })
                    .To<PublicField<string>>()
                    .UseNamePattern("^_(.+)_$");

                var source = new { Value = new { _Value_ = 999 } };
                var result = mapper.Map(source).ToANew<PublicProperty<PublicField<string>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBe("999");
            }
        }

        [Fact]
        public void ShouldUseACustomNamingSuffixPattern()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePattern("^(.+)__$");

                var source = new { Value__ = 878 };
                var result = mapper.Map(source).ToANew<PublicField<long>>();

                result.Value.ShouldBe(878);
            }
        }

        [Fact]
        public void ShouldUseACustomNamingSuffixPatternForGivenSourceAndTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { Value__ = default(int) })
                    .To<PublicField<string>>()
                    .UseNamePattern("^(.+)__$");

                var source = new { Value__ = 878 };
                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe("878");
            }
        }

        [Fact]
        public void ShouldUseCustomNamingPatterns()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.UseNamePatterns("^_abc(.+)xyz_$", "^__(.+)__$");

                var source = new { __Value__ = 456 };
                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldUseCustomNamingPatternsForGivenSourceAndTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { __Value__ = default(int) })
                    .To<PublicField<int>>()
                    .UseNamePatterns("^_abc(.+)xyz_$", "^__(.+)__$");

                var source = new { __Value__ = 456 };
                var result = mapper.Map(source).ToANew<PublicField<int>>();

                result.Value.ShouldBe(456);

                var nonMatchingSource = new { __Value__ = "456" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();

                nonMatchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldErrorIfInvalidNamePatternFormatSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseNamePatterns("^_[Name]_$");
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
                        .UseNamePatterns($@"{NewLine}^_{NewLine}(.+){NewLine}_$");
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
                    mapper.WhenMapping.UseNamePattern(null);
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
                    mapper.WhenMapping.UseNamePatterns();
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
                    mapper.WhenMapping.UseNamePattern("(.+)");
                }
            });
        }
    }
}