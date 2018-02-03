namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
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
        public void ShouldProjectToAComplexTypeMember()
        {
            RunTest(context =>
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

                context.Persons.Add(person);
                context.SaveChanges();

                var personDto = context.Persons.Project().To<PersonDto>().First();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Test Db");
                personDto.AddressId.ShouldBe(person.Address.AddressId);
                personDto.AddressLine1.ShouldBe("Test Db Line 1");
                personDto.AddressLine2.ShouldBe("Test Db Line 2");
            });
        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            RunTest(context =>
            {
                var person = new Person { Name = "No Address!" };

                context.Persons.Add(person);
                context.SaveChanges();

                var personDto = context.Persons.Project().To<PersonDto>().First();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("No Address!");
                personDto.AddressId.ShouldBeDefault();
                personDto.AddressLine1.ShouldBeNull();
                personDto.AddressLine2.ShouldBeNull();
            });
        }
    }
}
