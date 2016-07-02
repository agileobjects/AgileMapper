namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSources : NonParallelTestsBase
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                Mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicProperty<string>>()
                    .Map("Static fun!")
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "Instance fun!" };
                var result = Mapper.Map(source).ToANew<PublicProperty<string>>();

                result.Value.ShouldBe("Static fun!");
            });
        }
    }
}
