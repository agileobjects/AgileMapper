namespace AgileObjects.AgileMapper.UnitTests.Orms.Recursion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
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
        public Task ShouldProjectAOneToOneRelationship()
        {
            return RunTest(async context =>
            {
                var company = new Company
                {
                    Name = "Acme",
                    HeadOffice = new Address { Line1 = "Acme Park", Postcode = "AC3 3ME" }
                };

                context.Companies.Add(company);
                await context.SaveChanges();

                var ceo = new Employee
                {
                    Name = "Mr Ceo",
                    DateOfBirth = DateTime.Today.AddYears(-21),
                    Address = new Address { Line1 = "Ceo Towers", Postcode = "CE0 0EC" },
                    CompanyId = company.Id,
                    Company = company
                };

                context.Employees.Add(ceo);
                await context.SaveChanges();

                company.CeoId = ceo.Id;
                company.Ceo = ceo;
                company.HeadOfficeId = company.HeadOffice.AddressId;

                await context.SaveChanges();

                var companyDto = context.Companies.Project().To<CompanyDto>().ShouldHaveSingleItem();

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

        #region Project One-to-Many

        protected Task DoShouldErrorProjectingAOneToManyRelationshipToZeroethRecursionDepth()
            => RunTestAndExpectThrow(context => ProjectAOneToManyRelationshipToRecursionDepth(0, context));

        protected Task DoShouldProjectAOneToManyRelationshipToFirstRecursionDepth()
            => RunTest(context => ProjectAOneToManyRelationshipToRecursionDepth(1, context));

        protected Task DoShouldProjectAOneToManyRelationshipToSecondRecursionDepth()
            => RunTest(context => ProjectAOneToManyRelationshipToRecursionDepth(2, context));

        protected Task DoShouldProjectAOneToManyRelationshipToDefaultRecursionDepth()
            => RunTest(context => ProjectAOneToManyRelationshipToRecursionDepth(null, context));

        protected async Task ProjectAOneToManyRelationshipToRecursionDepth(
            int? depth,
            TOrmContext context)
        {
            var topLevel = new Category { Name = "Top Level" };

            topLevel.AddSubCategories(
                new Category { Name = "Top > One" },
                new Category { Name = "Top > Two" },
                new Category { Name = "Top > Three" });

            context.Categories.Add(topLevel);

            if (depth > 0)
            {
                topLevel.SubCategories.First().AddSubCategories(
                    new Category { Name = "Top > One > One" },
                    new Category { Name = "Top > One > Two" });

                topLevel.SubCategories.Second().AddSubCategories(
                    new Category { Name = "Top > Two > One" },
                    new Category { Name = "Top > Two > Two" },
                    new Category { Name = "Top > Two > Three" });

                topLevel.SubCategories.Third().AddSubCategories(
                    new Category { Name = "Top > Three > One" });

                if (depth > 1)
                {
                    topLevel.SubCategories.Second().SubCategories.Second().AddSubCategories(
                        new Category { Name = "Top > Two > Two > One" },
                        new Category { Name = "Top > Two > Two > Two" });
                }
            }

            await context.SaveChanges();

            var query = depth.HasValue
                ? context.Categories
                    .Project().To<CategoryDto>(cfg => cfg.RecurseToDepth(depth.Value))
                : context.Categories
                    .Project().To<CategoryDto>();

            var topLevelDto = query
                .OrderBy(c => c.Id)
                .First(c => c.Name == "Top Level");

            topLevelDto.Id.ShouldBe(topLevel.Id);
            topLevelDto.ParentCategoryId.ShouldBeNull();
            topLevelDto.ParentCategory.ShouldBeNull();

            var depth1Dtos = GetOrderedSubCategories(topLevelDto);

            depth1Dtos.Length.ShouldBe(3);

            var child1 = topLevel.SubCategories.First();
            var child2 = topLevel.SubCategories.Second();
            var child3 = topLevel.SubCategories.Third();

            Verify(depth1Dtos.First(), child1);
            Verify(depth1Dtos.Second(), child2);
            Verify(depth1Dtos.Third(), child3);

            if (!(depth > 0))
            {
                depth1Dtos.First().SubCategories.ShouldBeEmpty();
                depth1Dtos.Second().SubCategories.ShouldBeEmpty();
                depth1Dtos.Third().SubCategories.ShouldBeEmpty();
                return;
            }

            var depth11Dtos = GetOrderedSubCategories(depth1Dtos.First());

            depth11Dtos.Length.ShouldBe(2);

            Verify(depth11Dtos.First(), child1.SubCategories.First());
            Verify(depth11Dtos.Second(), child1.SubCategories.Second());

            var depth12Dtos = GetOrderedSubCategories(depth1Dtos.Second());

            depth12Dtos.Length.ShouldBe(3);

            Verify(depth12Dtos.First(), child2.SubCategories.First());
            Verify(depth12Dtos.Second(), child2.SubCategories.Second());

            var depth13Dtos = GetOrderedSubCategories(depth1Dtos.Third());

            depth13Dtos.ShouldHaveSingleItem();

            Verify(depth13Dtos.First(), child3.SubCategories.First());

            depth11Dtos.First().SubCategories.ShouldBeEmpty();
            depth11Dtos.Second().SubCategories.ShouldBeEmpty();

            depth12Dtos.First().SubCategories.ShouldBeEmpty();
            depth12Dtos.Third().SubCategories.ShouldBeEmpty();

            depth13Dtos.First().SubCategories.ShouldBeEmpty();

            if (!(depth > 1))
            {
                depth12Dtos.Second().SubCategories.ShouldBeEmpty();
                return;
            }

            var depth122Dtos = GetOrderedSubCategories(depth12Dtos.Second());

            depth122Dtos.Length.ShouldBe(2);

            Verify(depth122Dtos.First(), child2.SubCategories.Second().SubCategories.First());
            Verify(depth122Dtos.Second(), child2.SubCategories.Second().SubCategories.Second());
        }

        private static CategoryDto[] GetOrderedSubCategories(CategoryDto parentDto)
            => parentDto.SubCategories.OrderBy(sc => sc.Id).ToArray();

        private static void Verify(CategoryDto result, Category source)
        {
            result.Id.ShouldBe(source.Id);
            result.Name.ShouldBe(source.Name);
            result.ParentCategoryId.ShouldBe(source.ParentCategoryId);
        }

        #endregion
    }
}
