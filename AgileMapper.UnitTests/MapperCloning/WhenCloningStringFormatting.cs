namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using TestClasses;
    using Xunit;

    public class WhenCloningStringFormatting
    {
        [Fact]
        public void ShouldCloneCustomDoubleFormatting()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .StringsFrom<double>(c => c.FormatUsing("0.000"));

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    var originalResult = originalMapper
                        .Map(new PublicProperty<double> { Value = 1 })
                        .ToANew<PublicField<string>>();

                    originalResult.Value.ShouldBe("1.000");

                    var clonedResult = clonedMapper
                        .Map(new PublicProperty<double> { Value = 2 })
                        .ToANew<PublicField<string>>();

                    clonedResult.Value.ShouldBe("2.000");
                }
            }
        }

        [Fact]
        public void ShouldOverrideCustomDecimalFormatting()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .StringsFrom<decimal>(c => c.FormatUsing("0.00"));

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .StringsFrom<decimal>(c => c.FormatUsing("0.000"));

                    var originalResult = originalMapper
                        .Map(new PublicProperty<decimal> { Value = 1 })
                        .ToANew<PublicField<string>>();

                    originalResult.Value.ShouldBe("1.00");

                    var clonedResult = clonedMapper
                        .Map(new PublicProperty<decimal> { Value = 1 })
                        .ToANew<PublicField<string>>();

                    clonedResult.Value.ShouldBe("1.000");
                }
            }
        }
    }
}
