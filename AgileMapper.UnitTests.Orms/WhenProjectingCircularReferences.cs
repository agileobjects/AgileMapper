namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingCircularReferences<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingCircularReferences(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAOneToOneRelationship()
        {
            RunTest(context =>
            {
                var company = new Company
                {
                    Name = "Acme",
                    HeadOffice = new Address { Line1 = "Acme Park", Postcode = "AC3 3ME" }
                };

                context.Companies.Add(company);
                context.SaveChanges();

                var ceo = new Employee
                {
                    Name = "Mr Ceo",
                    DateOfBirth = DateTime.Today.AddYears(-21),
                    Address = new Address { Line1 = "Ceo Towers", Postcode = "CE0 0EC" },
                    CompanyId = company.Id,
                    Company = company
                };

                context.Employees.Add(ceo);
                context.SaveChanges();

                company.CeoId = ceo.Id;
                company.Ceo = ceo;
                company.HeadOfficeId = company.HeadOffice.AddressId;

                context.SaveChanges();

                var companyDto = context.Companies.ProjectTo<CompanyDto>().ShouldHaveSingleItem();

                companyDto.Id.ShouldBe(company.Id);
                companyDto.Name.ShouldBe(company.Name);

                companyDto.HeadOffice.ShouldNotBeNull();
                companyDto.HeadOffice.Line1.ShouldBe("Acme Park");
                companyDto.HeadOffice.Postcode.ShouldBe("AC3 3ME");

                companyDto.Ceo.ShouldNotBeNull();
                companyDto.Ceo.Name.ShouldBe("Mr Ceo");
                companyDto.Ceo.DateOfBirth.ShouldBe(ceo.DateOfBirth);
                companyDto.Ceo.Company.ShouldBeNull();

                companyDto.Ceo.Address.ShouldNotBeNull();
                companyDto.Ceo.Address.Line1.ShouldBe("Ceo Towers");
                companyDto.Ceo.Address.Postcode.ShouldBe("CE0 0EC");
            });
        }
    }
}
