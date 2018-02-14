namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;
    using static TestClasses.Animal.AnimalType;

    public class WhenConfiguringDerivedTypesInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringDerivedTypesInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToConfiguredDerivedTypesInline()
        {
            return RunTest(async context =>
            {
                var fido = new Animal { Type = Dog, Name = "Fido" };
                var nelly = new Animal { Type = Elephant, Name = "Nelly" };

                await context.Animals.AddRangeAsync(fido, nelly);
                await context.SaveChangesAsync();

                var animalDtos = await context
                    .Animals
                    .Project().To<AnimalDtoBase>(cfg => cfg
                        .If(a => a.Type == Dog)
                        .MapTo<DogDto>()
                        .And
                        .If(a => a.Type == Elephant)
                        .MapTo<ElephantDto>())
                    .OrderBy(a => a.Id)
                    .ToArrayAsync();

                animalDtos.First().ShouldBeOfType<DogDto>();
                animalDtos.First().Id.ShouldBe(fido.Id);
                animalDtos.First().Name.ShouldBe("Fido");
                animalDtos.First().Sound.ShouldBe("Woof");

                animalDtos.Second().ShouldBeOfType<ElephantDto>();
                animalDtos.Second().Id.ShouldBe(nelly.Id);
                animalDtos.Second().Name.ShouldBe("Nelly");
                animalDtos.Second().Sound.ShouldBe("Trumpet");
            });
        }
    }
}
