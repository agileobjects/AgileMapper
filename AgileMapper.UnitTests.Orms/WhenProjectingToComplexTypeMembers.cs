namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingToComplexTypeMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingToComplexTypeMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeMember()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Test Db",
                    Address = new Address
                    {
                        Line1 = "Test Db Line 1",
                        Line2 = "Test Db Line 2"
                    }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                var personDto = context.Persons.Project().To<PersonDto>().First();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Test Db");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Id.ShouldBe(person.Address.AddressId);
                personDto.Address.Line1.ShouldBe("Test Db Line 1");
                personDto.Address.Line2.ShouldBe("Test Db Line 2");
            });
        }

        [Fact]
        public Task ShouldHandleANullComplexTypeMember()
        {
            return RunTest(async context =>
            {
                var person = new Person { Name = "No Address!" };

                await context.Persons.Add(person);
                await context.SaveChanges();

                var personDto = context.Persons.Project().To<PersonDto>().First();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("No Address!");

                if (QueryProviderNonEntityNullConstants)
                {
                    personDto.Address.ShouldBeNull();
                    return;
                }

                personDto.Address.ShouldNotBeNull();
                personDto.Address.Id.ShouldBeDefault();
                personDto.Address.Line1.ShouldBeNull();
                personDto.Address.Line2.ShouldBeNull();
                personDto.Address.Postcode.ShouldBeNull();
            });
        }

        public virtual bool QueryProviderNonEntityNullConstants => true;
    }
}
