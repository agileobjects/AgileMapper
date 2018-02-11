namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;
    using static TestClasses.Animal.AnimalType;

    public abstract class WhenConfiguringDerivedTypes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDerivedTypes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToConfiguredDerivedTypes()
        {
            return RunTest(async context =>
            {
                var fido = new Animal { Type = Dog, Name = "Fido", Sound = "Bark" };
                var nelly = new Animal { Type = Elephant, Name = "Nelly" };
                var kaa = new Animal { Type = Snake, Name = "Kaa" };
                var sparkles = new Animal { Name = "Sparkles", Sound = "Wheeeee!" };

                await context.Animals.AddRange(fido, nelly, kaa, sparkles);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Animal>()
                        .ProjectedTo<AnimalDtoBase>()
                        .If(a => a.Type == Dog)
                        .MapTo<DogDto>()
                        .And
                        .If(a => a.Type == Elephant)
                        .MapTo<ElephantDto>()
                        .And
                        .If(a => a.Type == Snake)
                        .MapTo<SnakeDto>();

                    var animalDtos = context
                        .Animals
                        .ProjectUsing(mapper).To<AnimalDtoBase>()
                        .OrderBy(a => a != null ? a.Id : 1000)
                        .ToArray();

                    animalDtos.First().ShouldBeOfType<DogDto>();
                    animalDtos.First().Id.ShouldBe(fido.Id);
                    animalDtos.First().Name.ShouldBe("Fido");
                    animalDtos.First().Sound.ShouldBe("Woof");

                    animalDtos.Second().ShouldBeOfType<ElephantDto>();
                    animalDtos.Second().Id.ShouldBe(nelly.Id);
                    animalDtos.Second().Name.ShouldBe("Nelly");
                    animalDtos.Second().Sound.ShouldBe("Trumpet");

                    animalDtos.Third().ShouldBeOfType<SnakeDto>();
                    animalDtos.Third().Id.ShouldBe(kaa.Id);
                    animalDtos.Third().Name.ShouldBe("Kaa");
                    animalDtos.Third().Sound.ShouldBe("Hiss");

                    animalDtos.Fourth().ShouldBeNull();
                }
            });
        }

        [Fact]
        public Task ShouldProjectToAFallbackDerivedType()
        {
            return RunTest(async context =>
            {
                var fido = new Animal { Type = Dog, Name = "Fido", Sound = "Bark" };
                var nelly = new Animal { Type = Elephant, Name = "Nelly", Sound = "HrrrRRRRRRR" };
                var sparkles = new Animal { Name = "Sparkles", Sound = "Wheeeee!" };

                await context.Animals.AddRange(fido, nelly, sparkles);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Animal>()
                        .ProjectedTo<AnimalDtoBase>()
                        .If(a => a.Type == Dog)
                        .MapTo<DogDto>()
                        .And
                        .If(a => a.Type != Dog)
                        .MapTo<AnimalDto>();

                    var animalDtos = context
                        .Animals
                        .ProjectUsing(mapper).To<AnimalDtoBase>()
                        .OrderBy(a => a.Id)
                        .ToArray();

                    animalDtos.First().ShouldBeOfType<DogDto>();
                    animalDtos.First().Id.ShouldBe(fido.Id);
                    animalDtos.First().Name.ShouldBe("Fido");
                    animalDtos.First().Sound.ShouldBe("Woof");

                    animalDtos.Second().ShouldBeOfType<AnimalDto>();
                    animalDtos.Second().Id.ShouldBe(nelly.Id);
                    animalDtos.Second().Name.ShouldBe("Nelly");
                    animalDtos.Second().Sound.ShouldBe("HrrrRRRRRRR");

                    animalDtos.Third().ShouldBeOfType<AnimalDto>();
                    animalDtos.Third().Id.ShouldBe(sparkles.Id);
                    animalDtos.Third().Name.ShouldBe("Sparkles");
                    animalDtos.Third().Sound.ShouldBe("Wheeeee!");
                }
            });
        }
    }
}
