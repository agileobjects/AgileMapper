namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using Common;
    using MoreTestClasses;
    using NetStandardPolyfills;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDerivedTypes : NonParallelTestsBase
    {
        [Fact]
        public void ShouldScanConfiguredAssembliesViaTheStaticApi()
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

        [Fact]
        public void ShouldScanConfiguredAssembliesViaTheInstanceApi()
        {
            TestThenReset(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .LookForDerivedTypesIn(typeof(Dog).GetAssembly(), typeof(Earthworm).GetAssembly());

                    var result = mapper
                        .Map(new { NumberOfLegs = 1000, SlitherNoise = "thththtth" })
                        .Over(new Earthworm() as AnimalBase);

                    result.NumberOfLegs.ShouldBe(1000);
                    ((Earthworm)result).SlitherNoise.ShouldBe("thththtth");
                }
            });
        }

        [Fact]
        public void ShouldSetAssembliesToScanGlobally()
        {
            TestThenReset(() =>
            {
                using (var mapper1 = Mapper.CreateNew())
                using (var mapper2 = Mapper.CreateNew())
                {
                    // Set assembly scanning on mapper1...
                    mapper1.WhenMapping
                        .LookForDerivedTypesIn(typeof(Dog).GetAssembly(), typeof(Earthworm).GetAssembly());

                    // ...use mapper2 to cache the assembly scan results... 
                    var result1 = mapper2
                        .Map(new { NumberOfLegs = 4, WoofSound = "AWESOME" })
                        .OnTo(new Dog() as AnimalBase);

                    result1.NumberOfLegs.ShouldBe(4);
                    ((Dog)result1).WoofSound.ShouldBe("AWESOME");

                    // ...use mapper1 with a type outside the base type's assembly;
                    // assemblies are cached globally, so the scan settings should be too:
                    var result2 = mapper1
                        .Map(new { NumberOfLegs = 100, SlitherNoise = "SLITHERRR" })
                        .OnTo(new Earthworm() as AnimalBase);

                    result2.NumberOfLegs.ShouldBe(100);
                    ((Earthworm)result2).SlitherNoise.ShouldBe("SLITHERRR");
                }
            });
        }
    }
}
