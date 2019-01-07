namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration.Inline
{
    using Common;
    using MoreTestClasses;
    using NetStandardPolyfills;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypesInline : NonParallelTestsBase
    {
        [Fact]
        public void ShouldScanConfiguredAssembliesInline()
        {
            TestThenReset(() =>
            {
                var result = Mapper
                    .Map(new { NumberOfLegs = 100, SlitherNoise = "ththtth" })
                    .Over(new Earthworm() as AnimalBase, cgf => cgf
                        .LookForDerivedTypesIn(typeof(Dog).GetAssembly(), typeof(Earthworm).GetAssembly()));

                result.NumberOfLegs.ShouldBe(100);
                ((Earthworm)result).SlitherNoise.ShouldBe("ththtth");
            });
        }
    }
}
