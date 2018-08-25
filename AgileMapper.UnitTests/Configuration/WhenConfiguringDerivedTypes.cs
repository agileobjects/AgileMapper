namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Extensions.Internal;
    using Common;
    using MoreTestClasses;
    using NetStandardPolyfills;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDerivedTypes
    {
        [Fact]
        public void ShouldScanConfiguredAssemblies()
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
        }

        [Fact]
        public void ShouldSetAssembliesToScanGlobally()
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
        }

        [Fact]
        public void ShouldMapACustomTypePair()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .Map<MegaProduct>()
                    .To<ProductDtoMega>();

                Product source = new MegaProduct { ProductId = "PrettyDarnMega", Price = 0.99, HowMega = 1.00m };

                var result = mapper.Map(source).ToANew<ProductDto>();

                result.ShouldBeOfType<ProductDtoMega>();
                result.ProductId.ShouldBe("PrettyDarnMega");
                result.Price.ShouldBe(0.99m);
                ((ProductDtoMega)result).HowMega.ShouldBe("1.00");
            }
        }

        [Fact]
        public void ShouldMapADerivedTypePairConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var exampleInstance = new { Name = default(string), Discount = default(decimal?), Report = default(string) };

                mapper.WhenMapping
                    .From(exampleInstance)
                    .ToANew<PersonViewModel>()
                    .If(s => s.Source.Discount.HasValue)
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(x => !x.Source.Report.IsNullOrWhiteSpace())
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new
                {
                    Name = "???",
                    Discount = (decimal?).5m,
                    Report = "Lovely!"
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                mysteryCustomerResult.Name.ShouldBe("???");
                ((CustomerViewModel)mysteryCustomerResult).Discount.ShouldBe(0.5);
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Lovely!");

                var customerSource = new
                {
                    Name = "Firsty",
                    Discount = (decimal?)1,
                    Report = string.Empty
                };

                var customerResult = mapper.Map(customerSource).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                customerResult.Name.ShouldBe("Firsty");
                ((CustomerViewModel)customerResult).Discount.ShouldBe(1.0);

                var personSource = new
                {
                    Name = "Datey",
                    Discount = default(decimal?),
                    Report = default(string)
                };

                var personResult = mapper.Map(personSource).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Datey");
            }
        }
    }
}
