namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration.Inline
{
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringDataSourcesInline<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSourcesInline(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantInline()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                await context.Products.Add(product);
                await context.SaveChanges();

                var productDto = context
                    .Products
                    .Project().To<ProductDto>(cfg => cfg
                        .Map("PROD!!")
                        .To(dto => dto.Name))
                    .ShouldHaveSingleItem();

                productDto.ProductId.ShouldBe(product.ProductId);
                productDto.Name.ShouldBe("PROD!!");
            });
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantToANestedMemberInline()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Person One",
                    Address = new Address { Line1 = "Line One", Postcode = "Postcode" }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                var personDto = context
                    .Persons
                    .Project().To<PersonDto>(cfg => cfg
                        .WhenMapping
                        .ProjectionsTo<AddressDto>()
                        .Map("LINE TWO?!")
                        .To(a => a.Line2))
                    .ShouldHaveSingleItem();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Person One");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Line1.ShouldBe("Line One");
                personDto.Address.Line2.ShouldBe("LINE TWO?!");
                personDto.Address.Postcode.ShouldBe("Postcode");
            });
        }
    }
}
