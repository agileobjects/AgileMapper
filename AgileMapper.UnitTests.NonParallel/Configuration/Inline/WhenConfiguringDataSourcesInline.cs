namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration.Inline
{
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSourcesInline : NonParallelTestsBase
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantViaTheInlineStaticApi()
        {
            TestThenReset(() =>
            {
                var result = Mapper
                    .DeepClone(new PublicPropertyStruct<string> { Value = "Instance fun!" }, cfg => cfg
                        .Map("Static fun!")
                        .To(pps => pps.Value));

                result.Value.ShouldBe("Static fun!");
            });
        }
    }
}
