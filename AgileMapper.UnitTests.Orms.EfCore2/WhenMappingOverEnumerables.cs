namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverEnumerables : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenMappingOverEnumerables(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUpdateADbSetLocalCollection()
        {
            return RunTest(async (writeContext, mapper) =>
            {
                await writeContext.Persons.AddRangeAsync(
                    new Person { Name = "One", Address = new Address { Line1 = "One Line 1", Line2 = "One Line 2" } },
                    new Person { Name = "Two" },
                    new Person { Name = "Three", Address = new Address { Line1 = "Three Line 1" } },
                    new Person { Name = "Four" });

                await writeContext.SaveChangesAsync();

                using (var readContext = new EfCore2TestDbContext())
                {
                    await readContext.Persons.LoadAsync();

                    var localPersons = readContext.Persons.Local;

                    var orderedPersons = localPersons.OrderBy(p => p.PersonId).ToArray();

                    orderedPersons.Length.ShouldBe(4);
                    orderedPersons.First().Name.ShouldBe("One");
                    orderedPersons.Second().Name.ShouldBe("Two");
                    orderedPersons.Third().Name.ShouldBe("Three");
                    orderedPersons.Fourth().Name.ShouldBe("Four");

                    mapper.WhenMapping.InstancesOf<Person>().IdentifyUsing(p => p.Name);

                    var updatedPersons = new[]
                    {
                        new Person { Name = "One" },
                        new Person { Name = "Two", Address = new Address { Line1 = "Two Line 1" } },
                        new Person { Name = "Three", Address = new Address { Line1 = "Three Line 1", Line2 = "Three Line 2" } },
                        new Person { Name = "Five", Address = new Address { Line1 = "Five Line 1" } }
                    };

                    mapper.Map(updatedPersons).Over(localPersons);

                    await readContext.SaveChangesAsync();

                    orderedPersons = localPersons.OrderBy(p => p.PersonId).ToArray();

                    orderedPersons.Length.ShouldBe(4);
                    orderedPersons.First().Name.ShouldBe("One");
                    orderedPersons.First().Address.ShouldBeNull();

                    orderedPersons.Second().Name.ShouldBe("Two");
                    orderedPersons.Second().Address.ShouldNotBeNull();
                    orderedPersons.Second().Address.Line1.ShouldBe("Two Line 1");
                    orderedPersons.Second().Address.Line2.ShouldBeNull();

                    orderedPersons.Third().Name.ShouldBe("Three");
                    orderedPersons.Third().Address.ShouldNotBeNull();
                    orderedPersons.Third().Address.Line1.ShouldBe("Three Line 1");
                    orderedPersons.Third().Address.Line2.ShouldBe("Three Line 2");

                    orderedPersons.Fourth().Name.ShouldBe("Five");
                    orderedPersons.Fourth().Address.ShouldNotBeNull();
                    orderedPersons.Fourth().Address.Line1.ShouldBe("Five Line 1");
                    orderedPersons.Fourth().Address.Line2.ShouldBeNull();
                }
            });
        }
    }
}
