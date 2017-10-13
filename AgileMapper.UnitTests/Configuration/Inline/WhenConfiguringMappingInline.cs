namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using TestClasses;
    using Xunit;

    public class WhenConfiguringMappingInline
    {
        [Fact]
        public void ShouldAllowInlineConstantDataSourceConfigurationViaStaticApi()
        {
            var source = new PublicProperty<int> { Value = 1 };

            var result = Mapper
                .Map(source)
                .ToANew<PublicField<int>>(c => c
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(pf => pf.Value));

            result.Value.ShouldBe(1 * 2);
        }
    }
}
