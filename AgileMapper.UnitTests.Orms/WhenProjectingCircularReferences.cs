namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Linq;
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

        [Fact]
        public void ShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
        {
            RunTest(context =>
            {
                var topLevel = new Category { Name = "Top Level" };
                var child1 = new Category { Name = "Top > One", ParentCategory = topLevel };
                var child2 = new Category { Name = "Top > Two", ParentCategory = topLevel };
                var child3 = new Category { Name = "Top > Three", ParentCategory = topLevel };

                var grandChild11 = new Category { Name = "Top > One > One", ParentCategory = child1 };
                var grandChild12 = new Category { Name = "Top > One > Two", ParentCategory = child1 };

                var grandChild21 = new Category { Name = "Top > Two > One", ParentCategory = child2 };
                var grandChild22 = new Category { Name = "Top > Two > Two", ParentCategory = child2 };
                var grandChild23 = new Category { Name = "Top > Two > Three", ParentCategory = child2 };

                var grandChild31 = new Category { Name = "Top > Three > One", ParentCategory = child3 };

                var greatGrandchild221 = new Category { Name = "Top > Two > Two > One", ParentCategory = grandChild22 };
                var greatGrandchild222 = new Category { Name = "Top > Two > Two > Two", ParentCategory = grandChild22 };

                context.Categories.Add(topLevel);
                context.Categories.Add(child1);
                context.Categories.Add(child2);
                context.Categories.Add(child3);
                context.Categories.Add(grandChild11);
                context.Categories.Add(grandChild12);
                context.Categories.Add(grandChild21);
                context.Categories.Add(grandChild22);
                context.Categories.Add(grandChild23);
                context.Categories.Add(grandChild31);
                context.Categories.Add(greatGrandchild221);
                context.Categories.Add(greatGrandchild222);

                context.SaveChanges();

                var topLevelDto = context
                    .Categories
                    .ProjectTo<CategoryDto>()
                    .OrderBy(c => c.Id)
                    .First(c => c.Name == "Top Level");

                topLevelDto.Id.ShouldBe(topLevel.Id);
                topLevelDto.ParentCategoryId.ShouldBe(default(int));
                topLevelDto.ParentCategory.ShouldBeNull();

                topLevelDto.SubCategories.Count().ShouldBe(3);

                topLevelDto.SubCategories.First().Id.ShouldBe(child1.Id);
                topLevelDto.SubCategories.First().Name.ShouldBe("Top > One");
                topLevelDto.SubCategories.First().ParentCategoryId.ShouldBe(topLevel.Id);

                topLevelDto.SubCategories.Second().Id.ShouldBe(child2.Id);
                topLevelDto.SubCategories.Second().Name.ShouldBe("Top > Two");
                topLevelDto.SubCategories.Second().ParentCategoryId.ShouldBe(topLevel.Id);

                topLevelDto.SubCategories.Third().Id.ShouldBe(child3.Id);
                topLevelDto.SubCategories.Third().Name.ShouldBe("Top > Three");
                topLevelDto.SubCategories.Third().ParentCategoryId.ShouldBe(topLevel.Id);
            });
        }
    }
}
