namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using Infrastructure;
    using Shouldly;
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
                    PersonId = 1,
                    Name = "Test Db",
                    AddressId = 1,
                    Address = new Address
                    {
                        AddressId = 1,
                        Line1 = "Test Db Line 1",
                        Line2 = "Test Db Line 2"
                    }
                };

                context.Addresses.Add(person.Address);
                context.Persons.Add(person);
                context.SaveChanges();

                var personDto = context.Persons.ProjectTo<PersonDto>().First();

                personDto.Id.ShouldBe(1);
                personDto.Name.ShouldBe("Test Db");
                personDto.AddressId.ShouldBe(1);
                personDto.AddressLine1.ShouldBe("Test Db Line 1");
                personDto.AddressLine2.ShouldBe("Test Db Line 2");
            });
        }
    }
}
