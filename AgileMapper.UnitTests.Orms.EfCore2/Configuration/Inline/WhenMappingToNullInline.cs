namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNullInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenMappingToNullInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAUserConfigurationInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var person1 = new Person
                {
                    Name = "Frank",
                    Address = new Address { Line1 = "1" }
                };

                var person2 = new Person
                {
                    Name = "Dee",
                    Address = new Address { Line1 = "Paddie's Pub" }
                };

                await context.Persons.AddRangeAsync(person1, person2);
                await context.SaveChangesAsync();

                var personDtos = context
                    .Persons
                    .ProjectUsing(mapper)
                    .To<PersonDto>(cfg => cfg
                        .WhenMapping
                        .From<Address>()
                        .ProjectedTo<AddressDto>()
                        .If(a => a.Line1.Length != 1)
                        .MapToNull())
                    .OrderBy(p => p.Id)
                    .ToArray();

                personDtos.Length.ShouldBe(2);

                personDtos.First().Name.ShouldBe("Frank");
                personDtos.First().Address.ShouldNotBeNull();
                personDtos.First().Address.Line1.ShouldBe("1");

                personDtos.Second().Name.ShouldBe("Dee");
                personDtos.Second().Address.ShouldBeNull();
            });
        }
    }
}
