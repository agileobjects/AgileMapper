namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using Common;
    using MoreTestClasses;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypes : NonParallelTestsBase
    {
        [Fact]
        public void ShouldScanConfiguredAssemblies()
        {
            TestThenReset(() =>
            {
                Mapper.WhenMapping
                    .LookForDerivedTypesIn(typeof(Dog).Assembly, typeof(Earthworm).Assembly);

                var dogResult = Mapper
                    .Map(new { NumberOfLegs = 4, WoofSound = "Bark!" })
                    .OnTo(new Dog() as AnimalBase);

                dogResult.NumberOfLegs.ShouldBe(4);
                ((Dog)dogResult).WoofSound.ShouldBe("Bark!");

                var wormResult = Mapper
                    .Map(new { NumberOfLegs = 0, SlitherNoise = "sssSSS" })
                    .Over(new Earthworm() as AnimalBase);

                wormResult.NumberOfLegs.ShouldBe(0);
                ((Earthworm)wormResult).SlitherNoise.ShouldBe("sssSSS");
            });
        }
    }
}
