namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
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
        public void ShouldHandleACustomSuffix()
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
    }
}
